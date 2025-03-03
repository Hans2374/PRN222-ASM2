using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;

        public AppointmentService()
        {

            _appointmentRepository = new();
        }


        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _appointmentRepository.GetAllAsync();

        }
    }
}
