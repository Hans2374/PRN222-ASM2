using Microsoft.EntityFrameworkCore;
using PaymentCVSTS.Repositories.Basic;
using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>
    {
        public AppointmentRepository() { }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            // Use eager loading to include any related entities if needed
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            return appointment;
        }
    }
}