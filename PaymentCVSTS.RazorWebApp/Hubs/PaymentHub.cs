using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.RazorWebApp.Hubs
{
    public class PaymentHub : Hub
    {
        private readonly IPaymentService _paymentService;

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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}