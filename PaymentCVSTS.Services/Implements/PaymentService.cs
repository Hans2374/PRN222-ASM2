using Microsoft.Extensions.Logging;
using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ILogger<PaymentService> logger = null)
        {
            _paymentRepository = new PaymentRepository();
            _logger = logger;
        }

        public async Task<int> Create(Payment payment)
        {
            try
            {
                _logger?.LogInformation("Creating new payment for AppointmentId: {appointmentId}", payment.AppointmentId);
                var result = await _paymentRepository.CreateAsync(payment);
                return payment.PaymentId; // Return the PaymentId after creation
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating payment");
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                _logger?.LogInformation("Deleting payment with ID: {id}", id);
                var item = await _paymentRepository.GetByIdAsync(id);
                if (item != null)
                {
                    return await _paymentRepository.RemoveAsync(item);
                }
                _logger?.LogWarning("Payment with ID {id} not found for deletion", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting payment with ID: {id}", id);
                throw;
            }
        }

        public async Task<List<Payment>> GetAll()
        {
            try
            {
                _logger?.LogInformation("Getting all payments");
                return await _paymentRepository.GetAll();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all payments");
                throw;
            }
        }

        public async Task<Payment> GetById(int id)
        {
            try
            {
                _logger?.LogInformation("Getting payment by ID: {id}", id);
                return await _paymentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting payment by ID: {id}", id);
                throw;
            }
        }

        public async Task<List<Payment>> Search(DateOnly? date, string? status, string? method)
        {
            try
            {
                _logger?.LogInformation("Searching payments with filters: Date={date}, Status={status}, Method={method}",
                    date, status, method);
                return await _paymentRepository.Search(date, status, method);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching payments");
                throw;
            }
        }

        public async Task<int> Update(Payment payment)
        {
            try
            {
                _logger?.LogInformation("Updating payment with ID: {id}", payment.PaymentId);
                return await _paymentRepository.UpdateAsync(payment);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating payment with ID: {id}", payment.PaymentId);
                throw;
            }
        }
    }
}