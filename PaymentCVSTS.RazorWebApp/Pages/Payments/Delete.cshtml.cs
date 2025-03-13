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
    [Authorize] // Changed from [Authorize(Roles = "2")] to allow all authenticated users
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

            return RedirectToPage("./Index");
        }
    }
}