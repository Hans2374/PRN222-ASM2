// Modify your DeleteModel class in Payments/Delete.cshtml.cs:

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PaymentCVSTS.RazorWebApp.Hubs;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "2")]
    public class DeleteModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IHubContext<PaymentHub> _hubContext;

        public DeleteModel(IPaymentService paymentService, IHubContext<PaymentHub> hubContext)
        {
            _paymentService = paymentService;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Payment Payment { get; set; } = default!;

        // Add these properties to store the filter state
        [BindProperty(SupportsGet = true)]
        public string? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ChildId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var payment = await _paymentService.GetById(id);

            if (payment == null)
            {
                return NotFound();
            }

            Payment = payment;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var success = await _paymentService.Delete(id);

            if (success)
            {
                // Notify clients of the deletion via SignalR
                await _hubContext.Clients.All.SendAsync("Receive_DeletePayment", id);
            }

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