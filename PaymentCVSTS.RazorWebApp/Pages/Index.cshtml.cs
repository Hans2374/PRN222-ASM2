using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaymentCVSTS.RazorWebApp.Pages
{
    [Authorize] // Changed from [Authorize(Roles = "3,2,1")] to allow any authenticated user
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // You can add any initialization logic here if needed
        }
    }
}