// File: PaymentCVSTS.RazorWebApp/Pages/Index.cshtml.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaymentCVSTS.RazorWebApp.Pages
{
    [Authorize] // Allow any authenticated user
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Only redirect admins to Payments page
            if (User.IsInRole("1"))
            {
                // Admin - redirect to Payments
                return RedirectToPage("/Payments/Index");
            }

            // For all other roles, just show home page
            return Page();
        }
    }
}