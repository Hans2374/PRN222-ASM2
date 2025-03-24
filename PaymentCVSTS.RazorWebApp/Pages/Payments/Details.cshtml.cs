using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "1")]
    public class DetailsModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IPaymentService paymentService, ILogger<DetailsModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

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
                _logger.LogInformation("Fetching payment details for ID: {id}", id);
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
                _logger.LogError(ex, "Error retrieving payment details for ID {id}", id);
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnGetPaymentJsonAsync(int id)
        {
            try
            {
                var payment = await _paymentService.GetById(id);

                if (payment == null)
                {
                    return NotFound();
                }

                return new JsonResult(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment JSON for ID {id}", id);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}