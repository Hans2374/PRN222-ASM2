using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PaymentCVSTS.Repositories.Models;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Enums;
using PaymentCVSTS.RazorWebApp.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentCVSTS.RazorWebApp.Pages.Payments
{
    [Authorize(Roles = "1")]
    public class EditModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppointmentService _appointmentService;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IPaymentService paymentService,
            IAppointmentService appointmentService,
            IHubContext<PaymentHub> hubContext,
            ILogger<EditModel> logger)
        {
            _paymentService = paymentService;
            _appointmentService = appointmentService;
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
            _logger.LogInformation($"Initializing Edit Payment page for ID: {id}");

            if (id <= 0)
            {
                return NotFound("Payment ID is required");
            }

            try
            {
                var payment = await _paymentService.GetById(id);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment with ID {id} not found");
                    return NotFound($"Payment with ID {id} not found");
                }

                Payment = payment;

                // Load dropdowns
                await LoadDropdownsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading payment with ID {id}");
                ModelState.AddModelError("", $"Error loading payment: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation($"Attempting to update payment with ID: {Payment?.PaymentId}");

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

                try
                {
                    await _paymentService.Update(Payment);

                    // Notify clients about the update via SignalR
                    await _hubContext.Clients.All.SendAsync("Receive_UpdatePayment", Payment);
                    _logger.LogInformation($"Payment with ID {Payment.PaymentId} updated successfully");

                    return RedirectToPage("./Index", new
                    {
                        PaymentDate = this.PaymentDate,
                        PaymentStatus = this.PaymentStatus,
                        PaymentMethod = this.PaymentMethod
                    });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"Concurrency error updating payment {Payment.PaymentId}");

                    // Check if the payment still exists
                    var existingPayment = await _paymentService.GetById(Payment.PaymentId);
                    if (existingPayment == null)
                    {
                        return NotFound($"Payment with ID {Payment.PaymentId} no longer exists");
                    }

                    ModelState.AddModelError("", "Another user has modified this payment. Please try again.");
                    await LoadDropdownsAsync();
                    return Page();
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
                _logger.LogError(ex, "Error updating payment");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            try
            {
                _logger.LogInformation("Loading dropdowns for payment editing");

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
                        "DisplayText",
                        Payment?.AppointmentId
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
                    .Select(e => new { Id = e.ToString(), Name = e.ToString() }), "Id", "Name", Payment?.PaymentStatus);

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