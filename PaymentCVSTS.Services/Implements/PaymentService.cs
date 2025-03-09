using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;

        public PaymentService()
        {
            _paymentRepository = new PaymentRepository();
        }

        public async Task<int> Create(Payment payment)
        {
            var result = await _paymentRepository.CreateAsync(payment);
            return payment.PaymentId; // Return the PaymentId after creation
        }

        public async Task<bool> Delete(int id)
        {
            var item = await _paymentRepository.GetByIdAsync(id);
            if (item != null)
            {
                return await _paymentRepository.RemoveAsync(item);
            }
            return false;
        }

        public async Task<List<Payment>> GetAll()
        {
            return await _paymentRepository.GetAll();
        }

        public async Task<Payment> GetById(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<List<Payment>> Search(DateOnly? date, string? status, int? childId)
        {
            return await _paymentRepository.Search(date, status, childId);
        }

        public async Task<int> Update(Payment payment)
        {
            return await _paymentRepository.UpdateAsync(payment);
        }
    }
}