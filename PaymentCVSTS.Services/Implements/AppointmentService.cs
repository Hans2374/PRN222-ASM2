using PaymentCVSTS.Repositories;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.Services.Implements
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;

        public AppointmentService()
        {
            _appointmentRepository = new AppointmentRepository();
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _appointmentRepository.GetAllAsync();
        }

        public async Task<Appointment> GetById(int id)
        {
            return await _appointmentRepository.GetByIdAsync(id);
        }
    }
}