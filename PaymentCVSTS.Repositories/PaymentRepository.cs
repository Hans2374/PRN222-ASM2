using Microsoft.EntityFrameworkCore;
using PaymentCVSTS.Repositories.Basic;
using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>
    {
        public PaymentRepository() { }

        public async Task<List<Payment>> GetAll()
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .ToListAsync();
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            return payment;
        }

        public async Task<List<Payment>> Search(DateOnly? date, string? status, string? method)
        {
            var query = _context.Payments
                .Include(p => p.Appointment)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(p => p.PaymentDate == date.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PaymentStatus.Contains(status));
            }

            if (!string.IsNullOrEmpty(method))
            {
                query = query.Where(p => p.PaymentMethod.Contains(method));
            }

            return await query.ToListAsync();
        }
    }
}