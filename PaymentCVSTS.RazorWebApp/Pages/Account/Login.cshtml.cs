using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using PaymentCVSTS.Services.Interfaces;

namespace PaymentCVSTS.RazorWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Password { get; set; } = string.Empty;

        private readonly IUserAccountService _userAccountService;

        public LoginModel(IUserAccountService userAccountService) => _userAccountService = userAccountService;

        public void OnGet()
        {
            // If user is already authenticated, we could redirect to index page
            // if (User.Identity!.IsAuthenticated)
            // {
            //     return RedirectToPage("/Index");
            // }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userAccount = await _userAccountService.Authenticate(UserName, Password);

                if (userAccount != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userAccount.FullName),
                        new Claim(ClaimTypes.Role, userAccount.RoleId.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    Response.Cookies.Append("UserName", userAccount.FullName);
                    Response.Cookies.Append("Role", userAccount.RoleId.ToString());

                    return RedirectToPage("/Payments/Index");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ModelState.AddModelError("", "Login failure");
            return Page();
        }

        public async Task<IActionResult> OnGetLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Account/Login");
        }
    }
}