﻿using PaymentCVSTS.Repositories.Models;

namespace PaymentCVSTS.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAsync();
    }
}