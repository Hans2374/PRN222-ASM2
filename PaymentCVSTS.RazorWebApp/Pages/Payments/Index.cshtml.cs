using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "3,2")]
    public class IndexModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;

        public IndexModel(IPaymentService paymentService, IAppointmentService appointmentService)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
        }

        public IList<Payment> Payments { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public DateOnly? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ChildId { get; set; }

        public async Task OnGetAsync()
        {
            // Create a SelectList for payment statuses
            ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name");

            // Get payments based on search criteria
            Payments = await _paymentService.Search(PaymentDate, PaymentStatus, ChildId);
        }
    }
}