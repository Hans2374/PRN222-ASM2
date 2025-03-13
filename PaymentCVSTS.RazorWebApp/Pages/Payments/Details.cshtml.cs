using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize] // Changed from [Authorize(Roles = "3,2")] to allow all authenticated users
    public class DetailsModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public DetailsModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

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

        public async Task<IActionResult> OnGetPaymentJsonAsync(int id)
        {
            var payment = await _paymentService.GetById(id);

            if (payment == null)
            {
                return NotFound();
            }

            return new JsonResult(payment);
        }
    }
}