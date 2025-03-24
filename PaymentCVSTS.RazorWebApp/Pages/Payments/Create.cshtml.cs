using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "1")]
    public class CreateModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IPaymentService paymentService, IAppointmentService appointmentService, ILogger<CreateModel> logger)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // Add these properties to store the filter state
        [BindProperty(SupportsGet = true)]
        public string? PaymentDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentMethod { get; set; }

        [BindProperty]
        public Payment Payment { get; set; } = new Payment();

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Initializing Create Payment page");

            try
            {
                // Initialize a new payment object
                Payment = new Payment();

                // Explicitly set PaymentDate to null to prevent default date (01/01/0001)
                ModelState.SetModelValue("Payment.PaymentDate", null, null);

                // Load data for dropdowns
                await LoadDropdownsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Create Payment page");
                ModelState.AddModelError("", "An error occurred while initializing the page. Please try again.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Attempting to create a new payment");

            try
            {
                // Validate all required fields
                var validationErrors = new List<string>();

                if (Payment.Amount <= 0)
                {
                    ModelState.AddModelError("Payment.Amount", "Amount must be greater than zero");
                    validationErrors.Add("Amount must be greater than zero");
                }

                if (string.IsNullOrEmpty(Payment.PaymentStatus))
                {
                    ModelState.AddModelError("Payment.PaymentStatus", "Payment Status is required");
                    validationErrors.Add("Payment Status is required");
                }

                if (string.IsNullOrEmpty(Payment.PaymentMethod))
                {
                    ModelState.AddModelError("Payment.PaymentMethod", "Payment Method is required");
                    validationErrors.Add("Payment Method is required");
                }

                // Appointment ID must be selected
                if (Payment.AppointmentId <= 0)
                {
                    ModelState.AddModelError("Payment.AppointmentId", "Appointment is required");
                    validationErrors.Add("Appointment is required");
                }

                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return Page();
                }

                // Try to save the payment to database
                try
                {
                    await _paymentService.Create(Payment);
                    _logger.LogInformation($"Payment created successfully for AppointmentId: {Payment.AppointmentId}");
                    return RedirectToPage("./Index", new
                    {
                        PaymentDate = this.PaymentDate,
                        PaymentStatus = this.PaymentStatus,
                        PaymentMethod = this.PaymentMethod
                    });
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException?.Message ?? ex.Message;
                    _logger.LogError(ex, $"Database error: {errorMessage}");

                    ModelState.AddModelError("", $"Database error: {errorMessage}");
                    await LoadDropdownsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            try
            {
                _logger.LogInformation("Loading dropdowns for payment creation");

                // Get appointments for dropdown with service type
                var appointments = await _appointmentService.GetAllAsync();
                if (appointments != null && appointments.Any())
                {
                    // Format dropdown to show AppointmentId and ServiceType
                    ViewData["AppointmentId"] = new SelectList(
                        appointments.Select(a => new
                        {
                            a.AppointmentId,
                            DisplayText = $"ID: {a.AppointmentId} - {a.ServiceType ?? "No service type"}"
                        }),
                        "AppointmentId",
                        "DisplayText"
                    );
                }
                else
                {
                    ViewData["AppointmentId"] = new SelectList(new List<object>(), "AppointmentId", "DisplayText");
                    _logger.LogWarning("No appointments available for dropdown");
                }

                // Payment status dropdown
                ViewData["PaymentStatus"] = new SelectList(Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name");

                _logger.LogInformation("Dropdowns loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdowns");
                // Create empty select lists to avoid null reference exceptions
                ViewData["AppointmentId"] = new SelectList(new List<object>(), "AppointmentId", "AppointmentId");
                ViewData["PaymentStatus"] = new SelectList(new List<object>(), "Id", "Name");
            }
        }
    }
}