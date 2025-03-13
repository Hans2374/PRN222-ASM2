using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using PaymentCVSTS.Services.Implements;
using PaymentCVSTS.Services.Interfaces;
using PaymentCVSTS.RazorWebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR(e =>
{
    e.EnableDetailedErrors = true;
    e.MaximumReceiveMessageSize = 102400000;
});

// Register service interfaces and implementations
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Extended session timeout
    });

// Configure authorization policies to be more permissive
builder.Services.AddAuthorization(options =>
{
    // Default policy only requires authentication, not specific roles
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<PaymentHub>("/paymentHub");

app.Run();