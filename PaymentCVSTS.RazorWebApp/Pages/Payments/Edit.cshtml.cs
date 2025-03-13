using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using PaymentCVSTS.RazorWebApp.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize] // Changed from [Authorize(Roles = "3,2")] to allow all authenticated users
    public class EditModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;
        private readonly IHubContext<PaymentHub> _hubContext;

        public EditModel(IPaymentService paymentService, IAppointmentService appointmentService, IHubContext<PaymentHub> hubContext)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Payment Payment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var payment = await _paymentService.GetById(id);

            if (payment == null)
            {
                return NotFound();
            }

            Payment = payment;

            // Populate dropdowns
            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(appointments, "AppointmentId", "AppointmentId", Payment.AppointmentId);

            ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name", Payment.PaymentStatus);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var appointments = await _appointmentService.GetAllAsync();
                ViewData["AppointmentId"] = new SelectList(appointments, "AppointmentId", "AppointmentId", Payment.AppointmentId);

                ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name", Payment.PaymentStatus);

                return Page();
            }

            try
            {
                await _paymentService.Update(Payment);

                // Notify clients about the update via SignalR
                await _hubContext.Clients.All.SendAsync("Receive_UpdatePayment", Payment);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _paymentService.GetById(Payment.PaymentId) != null;
                if (!exists)
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}