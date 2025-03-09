using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRespository;

        public PaymentService()
        {
            _paymentRespository = new PaymentRepository();
        }

        public async Task<int> Create(Payment payment)
        {
            return await _paymentRespository.CreateAsync(payment);
        }

        public async Task<bool> Delete(int id)
        {
            var item = await _paymentRespository.GetByIdAsync(id);
            if (item != null)
            {
                return await _paymentRespository.RemoveAsync(item);
            }
            return false;
        }

        public async Task<List<Payment>> GetAll()
        {
            return await _paymentRespository.GetAll();
        }

        public async Task<Payment> GetById(int id)
        {
            return await _paymentRespository.GetByIdAsync(id);
        }

        public async Task<List<Payment>> Search(DateOnly? date, string? status, int? childId)
        {
            return await _paymentRespository.Search(date, status, childId);
        }

        public async Task<int> Update(Payment payment)
        {
            return await _paymentRespository.UpdateAsync(payment);
        }
    }
}