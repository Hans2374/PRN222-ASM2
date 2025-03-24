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
    [Authorize(Roles = "1")]
    public class DeleteModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IPaymentService paymentService, IHubContext<PaymentHub> hubContext, ILogger<DeleteModel> logger)
        {
            _paymentService = paymentService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty]
        public Payment Payment { get; set; } = default!;

        // Store the filter state
        [BindProperty(SupportsGet = true)]
        public string? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentMethod { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Loading payment with ID {id} for deletion", id);
                var payment = await _paymentService.GetById(id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {id} not found", id);
                    return NotFound();
                }

                Payment = payment;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment for deletion with ID {id}", id);
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting payment with ID {id}", id);
                var success = await _paymentService.Delete(id);

                if (success)
                {
                    // Notify clients of the deletion via SignalR
                    await _hubContext.Clients.All.SendAsync("Receive_DeletePayment", id);
                    _logger.LogInformation("Payment with ID {id} successfully deleted", id);
                }

                // Pass along the filter parameters
                return RedirectToPage("./Index", new
                {
                    PaymentDate = this.PaymentDate,
                    PaymentStatus = this.PaymentStatus,
                    PaymentMethod = this.PaymentMethod
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID {id}", id);
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return Page();
            }
        }
    }
}