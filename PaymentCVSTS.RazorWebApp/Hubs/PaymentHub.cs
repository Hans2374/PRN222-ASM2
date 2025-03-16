// First, let's make sure the PaymentHub.cs is properly handling creates

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

                Console.WriteLine($"🔄 Sending payment ID {payment.PaymentId} to clients...");

                // First save to database
                await _paymentService.Create(payment);

                // Then broadcast to all clients 
                await Clients.All.SendAsync("Receive_Payment", payment);

                Console.WriteLine("✅ Payment sent to clients and saved to database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in SendPayment: " + ex.ToString());
            }
        }

        // Rest of the methods...
        public async Task UpdatePayment(string paymentJsonString)
        {
            try
            {
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);
                await _paymentService.Update(payment);
                await Clients.All.SendAsync("Receive_UpdatePayment", payment);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in UpdatePayment: " + ex.ToString());
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
                Console.WriteLine("❌ Error in DeletePayment: " + ex.ToString());
            }
        }
    }
}