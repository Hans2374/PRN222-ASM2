using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Hubs
{
    public class PaymentHub : Hub
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<PaymentHub> _logger;

        public PaymentHub(
            IPaymentService paymentService,
            IAppointmentService appointmentService,
            ILogger<PaymentHub> logger)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public async Task SendPayment(string paymentJsonString)
        {
            try
            {
                _logger.LogInformation("Received payment creation request: {json}", paymentJsonString);

                // Deserialize payment data
                var payment = JsonConvert.DeserializeObject<Payment>(paymentJsonString);

                if (payment == null)
                {
                    throw new ArgumentException("Invalid payment data");
                }

                // Parse the date if needed
                if (payment.PaymentDate == default)
                {
                    var dateString = JsonConvert.DeserializeObject<dynamic>(paymentJsonString)?.paymentDate?.ToString();
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        if (DateOnly.TryParse(dateString, out DateOnly parsedDate))
                        {
                            payment.PaymentDate = parsedDate;
                        }
                    }
                }

                // Verify appointment exists
                var appointment = await _appointmentService.GetById(payment.AppointmentId);
                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {payment.AppointmentId} not found");
                }

                // Save payment to database
                int newPaymentId = await _paymentService.Create(payment);
                _logger.LogInformation("Payment created with ID: {id}", newPaymentId);

                // Get the full payment with appointment data
                var fullPayment = await _paymentService.GetById(newPaymentId);

                if (fullPayment == null)
                {
                    throw new Exception($"Failed to retrieve the created payment with ID {newPaymentId}");
                }

                // Create a simplified payment DTO to avoid circular references
                var paymentDto = new
                {
                    paymentId = fullPayment.PaymentId,
                    amount = fullPayment.Amount,
                    paymentStatus = fullPayment.PaymentStatus,
                    paymentMethod = fullPayment.PaymentMethod,
                    paymentDate = fullPayment.PaymentDate.ToString("yyyy-MM-dd"),
                    appointmentId = fullPayment.AppointmentId,
                    appointment = fullPayment.Appointment != null ? new
                    {
                        appointmentId = fullPayment.Appointment.AppointmentId,
                        serviceType = fullPayment.Appointment.ServiceType,
                        totalCost = fullPayment.Appointment.TotalCost
                    } : null
                };

                // Broadcast to clients using the simplified DTO
                await Clients.All.SendAsync("Receive_Payment", paymentDto);
                _logger.LogInformation("Payment broadcast to clients successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        public async Task UpdatePayment(string paymentJsonString)
        {
            try
            {
                _logger.LogInformation("Processing payment update: {json}", paymentJsonString);

                // Parse the JSON directly for better control
                JObject paymentObj = JObject.Parse(paymentJsonString);

                // Extract values with proper type conversion
                int paymentId = paymentObj["paymentId"]?.Value<int>() ?? 0;
                decimal amount = paymentObj["amount"]?.Value<decimal>() ?? 0;
                string paymentStatus = paymentObj["paymentStatus"]?.Value<string>() ?? string.Empty;
                string paymentMethod = paymentObj["paymentMethod"]?.Value<string>() ?? string.Empty;
                string paymentDateStr = paymentObj["paymentDate"]?.Value<string>() ?? string.Empty;
                int appointmentId = paymentObj["appointmentId"]?.Value<int>() ?? 0;

                // Validate required values
                if (paymentId <= 0)
                {
                    throw new ArgumentException("Payment ID is required");
                }

                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be greater than zero");
                }

                if (string.IsNullOrEmpty(paymentStatus))
                {
                    throw new ArgumentException("Payment Status is required");
                }

                if (string.IsNullOrEmpty(paymentMethod))
                {
                    throw new ArgumentException("Payment Method is required");
                }

                if (string.IsNullOrEmpty(paymentDateStr))
                {
                    throw new ArgumentException("Payment Date is required");
                }

                if (appointmentId <= 0)
                {
                    throw new ArgumentException("Appointment ID is required");
                }

                // Parse the date
                DateOnly paymentDate;
                if (!DateOnly.TryParse(paymentDateStr, out paymentDate))
                {
                    throw new ArgumentException($"Invalid date format: {paymentDateStr}");
                }

                // Get the existing payment
                var existingPayment = await _paymentService.GetById(paymentId);
                if (existingPayment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {paymentId} not found");
                }

                // Verify appointment exists
                var appointment = await _appointmentService.GetById(appointmentId);
                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
                }

                // Create payment object with updated values
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    Amount = amount,
                    PaymentStatus = paymentStatus,
                    PaymentMethod = paymentMethod,
                    PaymentDate = paymentDate,
                    AppointmentId = appointmentId
                };

                // Update in database
                await _paymentService.Update(payment);
                _logger.LogInformation("Payment updated successfully: ID={id}", paymentId);

                // Get updated payment with related data
                var updatedPayment = await _paymentService.GetById(paymentId);

                // Create DTO for response
                var paymentDto = new
                {
                    paymentId = updatedPayment.PaymentId,
                    amount = updatedPayment.Amount,
                    paymentStatus = updatedPayment.PaymentStatus,
                    paymentMethod = updatedPayment.PaymentMethod,
                    paymentDate = updatedPayment.PaymentDate.ToString("yyyy-MM-dd"),
                    appointmentId = updatedPayment.AppointmentId,
                    appointment = updatedPayment.Appointment != null ? new
                    {
                        appointmentId = updatedPayment.Appointment.AppointmentId,
                        serviceType = updatedPayment.Appointment.ServiceType,
                        totalCost = updatedPayment.Appointment.TotalCost
                    } : null
                };

                // Broadcast update
                await Clients.All.SendAsync("Receive_UpdatePayment", paymentDto);
                _logger.LogInformation("Update broadcast successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment");
                throw;
            }
        }

        public async Task DeletePayment(int id)
        {
            try
            {
                _logger.LogInformation("Received payment deletion request for ID: {id}", id);

                // Get the original payment to ensure it exists
                var payment = await _paymentService.GetById(id);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {id} not found");
                }

                // Delete from database
                var result = await _paymentService.Delete(id);

                if (!result)
                {
                    throw new Exception($"Failed to delete payment with ID {id}");
                }

                _logger.LogInformation("Payment deleted with ID: {id}", id);

                // Broadcast to clients
                await Clients.All.SendAsync("Receive_DeletePayment", id);
                _logger.LogInformation("Payment deletion broadcast to clients successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment");
                throw;
            }
        }
    }
}