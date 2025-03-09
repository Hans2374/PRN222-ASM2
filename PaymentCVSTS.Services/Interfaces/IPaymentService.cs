using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetAll();
        Task<Payment> GetById(int id);
        Task<List<Payment>> Search(DateOnly? date, string? status, int? childId);
        Task<int> Create(Payment payment);
        Task<int> Update(Payment payment);
        Task<bool> Delete(int id);
    }
}