// Modify your CreateModel class in Payments/Create.cshtml.cs:

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "3,2")]
    public class CreateModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;

        public CreateModel(IPaymentService paymentService, IAppointmentService appointmentService)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
        }

        // Add these properties to store the filter state
        [BindProperty(SupportsGet = true)]
        public string? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ChildId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public async Task<IActionResult> OnGet()
        {
            // Get appointments for dropdown
            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(appointments, "AppointmentId", "AppointmentId");

            // Payment status dropdown
            ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name");

            return Page();
        }

        [BindProperty]
        public Payment Payment { get; set; } = default!;

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

            await _paymentService.Create(Payment);

            // Pass along the filter parameters
            return RedirectToPage("./Index", new
            {
                PaymentDate = this.PaymentDate,
                PaymentStatus = this.PaymentStatus,
                ChildId = this.ChildId,
                CurrentPage = this.CurrentPage
            });
        }
    }
}