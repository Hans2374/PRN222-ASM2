using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.RazorWebApp.Hubs
{
    public class PaymentHub : Hub
    {
        private readonly IPaymentService _paymentService;
        private static Dictionary<int, HashSet<EditingSession>> _activeEditors = new Dictionary<int, HashSet<EditingSession>>();

        // Record of who is editing what payment
        private class EditingSession
        {
            public string ConnectionId { get; set; }
            public string SessionId { get; set; }
            public string UserName { get; set; }
            public DateTime StartTime { get; set; }
        }

        public PaymentHub(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task SendPayment(string paymentJsonString)
        {
            try
            {
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);

                Console.WriteLine("🔄 Sending payment to clients...");
                await Clients.All.SendAsync("Receive_Payment", payment);
                Console.WriteLine("✅ Payment sent to clients!");

                // Save payment to database
                await _paymentService.Create(payment);
                Console.WriteLine("✅ Payment saved to database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in SendPayment: " + ex.ToString());
            }
        }

        public async Task UpdatePayment(string paymentJsonString)
        {
            try
            {
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);

                Console.WriteLine("🔄 Updating payment...");

                // Update payment in database
                await _paymentService.Update(payment);
                Console.WriteLine("✅ Payment updated in database!");

                // Notify all clients about the update
                await Clients.All.SendAsync("Receive_UpdatePayment", payment);
                Console.WriteLine("✅ Update notification sent to clients!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in UpdatePayment: " + ex.ToString());
                throw;
            }
        }

        public async Task DeletePayment(int id)
        {
            try
            {
                var success = await _paymentService.Delete(id);
                if (success)
                {
                    await Clients.All.SendAsync("Receive_DeletePayment", id);

                    // Clean up active editors for this payment
                    if (_activeEditors.ContainsKey(id))
                    {
                        _activeEditors.Remove(id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Real-time editing methods

        public async Task StartEditingPayment(int paymentId, string sessionId, string userName)
        {
            try
            {
                // Add user to active editors for this payment
                if (!_activeEditors.ContainsKey(paymentId))
                {
                    _activeEditors[paymentId] = new HashSet<EditingSession>();
                }

                _activeEditors[paymentId].Add(new EditingSession
                {
                    ConnectionId = Context.ConnectionId,
                    SessionId = sessionId,
                    UserName = userName,
                    StartTime = DateTime.UtcNow
                });

                // Notify all clients that this user started editing
                await Clients.All.SendAsync("UserStartedEditing", paymentId, sessionId, userName);

                // Send back the list of other active editors to this user
                var otherEditors = _activeEditors[paymentId]
                    .Where(e => e.SessionId != sessionId)
                    .Select(e => new { e.SessionId, e.UserName })
                    .ToList();

                await Clients.Caller.SendAsync("ActiveEditors", paymentId, otherEditors);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in StartEditingPayment: " + ex.ToString());
            }
        }

        public async Task StopEditingPayment(int paymentId, string sessionId, string userName)
        {
            try
            {
                // Remove user from active editors for this payment
                if (_activeEditors.ContainsKey(paymentId))
                {
                    var editorToRemove = _activeEditors[paymentId]
                        .FirstOrDefault(e => e.SessionId == sessionId);

                    if (editorToRemove != null)
                    {
                        _activeEditors[paymentId].Remove(editorToRemove);

                        // Clean up if no editors left
                        if (_activeEditors[paymentId].Count == 0)
                        {
                            _activeEditors.Remove(paymentId);
                        }
                    }
                }

                // Notify all clients that this user stopped editing
                await Clients.All.SendAsync("UserStoppedEditing", paymentId, sessionId, userName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in StopEditingPayment: " + ex.ToString());
            }
        }

        public async Task BroadcastFieldChange(int paymentId, string sessionId, string userName, string fieldId, string fieldValue)
        {
            try
            {
                // Notify other clients about the field change
                await Clients.Others.SendAsync("FieldChanged", paymentId, sessionId, userName, fieldId, fieldValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in BroadcastFieldChange: " + ex.ToString());
            }
        }

        // Handle disconnections
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Find all payments this connection was editing
            var editingSessions = _activeEditors
                .SelectMany(kv => kv.Value
                    .Where(session => session.ConnectionId == Context.ConnectionId)
                    .Select(session => new { PaymentId = kv.Key, Session = session }))
                .ToList();

            // Notify others and clean up for each payment
            foreach (var item in editingSessions)
            {
                await Clients.Others.SendAsync("UserStoppedEditing",
                    item.PaymentId,
                    item.Session.SessionId,
                    item.Session.UserName);

                _activeEditors[item.PaymentId].Remove(item.Session);

                // Clean up if no editors left
                if (_activeEditors[item.PaymentId].Count == 0)
                {
                    _activeEditors.Remove(item.PaymentId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}