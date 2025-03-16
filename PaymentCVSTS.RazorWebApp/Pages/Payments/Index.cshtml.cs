// Modify your IndexModel class in Payments/Index.cshtml.cs to add sorting:

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
using System.Linq;

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

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; } = "PaymentDate"; // Default sort field

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc"; // Default sort direction

        public async Task OnGetAsync()
        {
            // Create a SelectList for payment statuses
            ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name");

            // Get all payments based on search criteria
            var payments = await _paymentService.Search(PaymentDate, PaymentStatus, ChildId);

            // Apply sorting
            payments = ApplySorting(payments);

            Payments = payments;
        }

        private List<Payment> ApplySorting(List<Payment> payments)
        {
            // Apply sorting based on the specified field and direction
            switch (SortField)
            {
                case "Amount":
                    payments = SortDirection == "asc"
                        ? payments.OrderBy(p => p.Amount).ToList()
                        : payments.OrderByDescending(p => p.Amount).ToList();
                    break;

                case "PaymentDate":
                    payments = SortDirection == "asc"
                        ? payments.OrderBy(p => p.PaymentDate).ToList()
                        : payments.OrderByDescending(p => p.PaymentDate).ToList();
                    break;

                default:
                    // Default sort by PaymentDate descending
                    payments = payments.OrderByDescending(p => p.PaymentDate).ToList();
                    break;
            }

            return payments;
        }
    }
}