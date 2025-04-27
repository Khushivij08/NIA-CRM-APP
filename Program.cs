using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using NIA_CRM.ViewModels;
using OfficeOpenXml;
using static NIA_CRM.Utilities.EmailService;
// Entry point for the web application
var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------
// 1. Read connection string from configuration
// --------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("NIACRMContext")
    ?? throw new InvalidOperationException("Connection string 'NIACRMContext' not found.");


// --------------------------------------------
// 2. Increase max request body size for IIS and Kestrel to allow large file uploads (100MB)
// --------------------------------------------
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

// --------------------------------------------
// 3. Set EPPlus license for Excel export/import functionality
// --------------------------------------------
ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use 'Commercial' if for commercial use

// --------------------------------------------
// 4. Register DbContext for Identity authentication system
// --------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// --------------------------------------------
// 5. Enable detailed exception pages for database errors during development
// --------------------------------------------
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --------------------------------------------
// 6. Configure Identity system (authentication + roles)
// --------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Enable role-based authorization
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Store identity data in the database

// --------------------------------------------
// 7. Customize password, lockout, and user settings for Identity
// --------------------------------------------
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Account lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// --------------------------------------------
// 8. Configure cookie behavior for authentication
// --------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Idle timeout of 20 minutes

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true; // Resets expiration time on user activity
});


// --------------------------------------------
// 9. Register your main application database context (CRM data)
// --------------------------------------------
builder.Services.AddDbContext<NIACRMContext>(options =>
    options.UseSqlite(connectionString));

// --------------------------------------------
// 10. Register HTTP client for calling NAICS API
// --------------------------------------------
builder.Services.AddHttpClient<NAICSApiHelper>();

// (Duplicate call to AddDatabaseDeveloperPageExceptionFilter – optional to keep)
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --------------------------------------------
// 11. Register MVC services (controllers + views)
// --------------------------------------------
builder.Services.AddControllersWithViews();

// --------------------------------------------
// 12. Enable and configure session state
// --------------------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Timeout after 20 minutes of inactivity
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});


// (Another duplicate call to AddDatabaseDeveloperPageExceptionFilter – can remove if needed)
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --------------------------------------------
// 13. Register email service configuration from appsettings.json
// --------------------------------------------
builder.Services.AddSingleton<IEmailConfiguration>(
    builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

// --------------------------------------------
// 14. Register email sending services for Identity
// --------------------------------------------
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Custom email service for production-level sending
builder.Services.AddTransient<IMyEmailSender, MyEmailSender>();

// Custom email service class
builder.Services.AddTransient<EmailService>();

// --------------------------------------------
// 15. Register IHttpContextAccessor for audit logging and other request-based operations
// --------------------------------------------
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// --------------------------------------------
// 16. Build the application
// --------------------------------------------
var app = builder.Build();

// --------------------------------------------
// 17. Configure the HTTP request pipeline
// --------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Enable migration endpoints in development
    app.UseMigrationsEndPoint();
}
else
{
    // Use error handler in production
    app.UseExceptionHandler("/Home/Error");
}

// Serve static files (CSS, JS, images, etc.)
app.UseStaticFiles();

// Set up routing middleware
app.UseRouting();

// Enable session usage
app.UseSession();

// Enable authorization (authentication is implicit)
app.UseAuthorization();

// Define default route for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Enable Razor Pages (for Identity UI)
app.MapRazorPages();

// --------------------------------------------
// 18. Initialize databases and seed sample data
// --------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Seed data for NIA CRM database
    NIACRMInitializer.Initialize(serviceProvider: services,
        DeleteDatabase: true,
        UseMigrations: true,
        SeedSampleData: true);

    // Seed data for Identity-related Application DB
    ApplicationDbInitializer.Initialize(serviceProvider: services,
        UseMigrations: true,
        SeedSampleData: true);
}

// --------------------------------------------
// 19. Run the application
// --------------------------------------------
app.Run();
