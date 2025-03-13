using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize] // Allow all authenticated users
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
        public IList<Payment> PaginatedPayments { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public DateOnly? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ChildId { get; set; }

        // Sorting properties - simplified to only amount and date
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        public string CurrentSort { get; set; }
        public string AmountSort { get; set; }
        public string DateSort { get; set; }

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 7;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            // Set up sort headers - only for amount and date
            AmountSort = SortOrder == "amount_asc" ? "amount_desc" : "amount_asc";
            DateSort = SortOrder == "date_asc" ? "date_desc" : "date_asc";
            CurrentSort = SortOrder;

            // Create a SelectList for payment statuses
            ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name");

            // Get payments based on search criteria
            Payments = await _paymentService.Search(PaymentDate, PaymentStatus, ChildId);

            // Apply sorting - only for amount and date
            switch (SortOrder)
            {
                case "amount_asc":
                    Payments = Payments.OrderBy(p => p.Amount).ToList();
                    break;
                case "amount_desc":
                    Payments = Payments.OrderByDescending(p => p.Amount).ToList();
                    break;
                case "date_asc":
                    Payments = Payments.OrderBy(p => p.PaymentDate).ToList();
                    break;
                case "date_desc":
                    Payments = Payments.OrderByDescending(p => p.PaymentDate).ToList();
                    break;
                default:
                    // Default sort - newest first
                    Payments = Payments.OrderByDescending(p => p.PaymentDate).ToList();
                    break;
            }

            // Apply pagination
            TotalPages = (int)Math.Ceiling(Payments.Count / (double)PageSize);

            // Ensure current page is within valid range
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            else if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }

            // Apply pagination
            PaginatedPayments = Payments
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}