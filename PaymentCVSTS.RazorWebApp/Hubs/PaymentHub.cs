using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.RazorWebApp.Hubs
{
    public class PaymentHub : Hub
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentHub> _logger;

        public PaymentHub(IPaymentService paymentService, ILogger<PaymentHub> logger = null)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task SendPayment(string paymentJsonString)
        {
            try
            {
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);

                _logger?.LogInformation($"🔄 Sending payment for AppointmentID {payment.AppointmentId} to clients...");

                // First save to database
                await _paymentService.Create(payment);

                // Then broadcast to all clients 
                await Clients.All.SendAsync("Receive_Payment", payment);

                _logger?.LogInformation("✅ Payment sent to clients and saved to database!");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error in SendPayment");
                throw;
            }
        }

        public async Task UpdatePayment(string paymentJsonString)
        {
            try
            {
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);
                await _paymentService.Update(payment);
                await Clients.All.SendAsync("Receive_UpdatePayment", payment);
                _logger?.LogInformation($"✅ Payment ID {payment.PaymentId} updated successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error in UpdatePayment");
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
                    _logger?.LogInformation($"✅ Payment ID {id} deleted successfully");
                }
                else
                {
                    _logger?.LogWarning($"⚠️ Failed to delete payment ID {id}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error in DeletePayment");
                throw;
            }
        }
    }
}