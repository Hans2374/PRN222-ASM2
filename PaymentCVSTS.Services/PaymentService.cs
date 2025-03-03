using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Services
{
    public interface IPayment
    {

        Task<List<Payment>> GetAll();

        Task<Payment> GetById(int id);
        Task<List<Payment>> Search(DateOnly? date, string? status, int? childId);
        Task<int> Create(Payment payment);

        Task<int> Update(Payment payment);
        Task<bool> Delete(int id);

    }
    public class PaymentService : IPayment
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
