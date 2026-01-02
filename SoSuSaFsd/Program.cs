using SoSuSaFsd.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoSuSaFsd.Data;
using Microsoft.AspNetCore.Identity; // Required for IdentityRole

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext Configuration
builder.Services.AddDbContextFactory<SoSuSaFsdContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoSuSaFsdContext") ?? throw new InvalidOperationException("Connection string 'SoSuSaFsdContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Add Identity Services
// UPDATED: Added password options inside this block
builder.Services.AddIdentityCore<SoSuSaFsd.Domain.Users>(options =>
{
    // Keep your existing setting
    options.SignIn.RequireConfirmedAccount = true;

    // --- NEW: RELAX PASSWORD REQUIREMENTS ---
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false; // No symbols required
    options.Password.RequiredLength = 1;             // Min length 1
    options.Password.RequiredUniqueChars = 0;
})
    .AddRoles<IdentityRole>()                 // Enables Role Management (Admin, User)
    .AddEntityFrameworkStores<SoSuSaFsdContext>() // Connects Identity to your DB
    .AddSignInManager()                       // Adds SignInManager support
    .AddDefaultTokenProviders();              // Support for tokens (reset password, etc.)

// 3. Add Authentication State (Required for <AuthorizeView> to work)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// 4. Enable Authentication & Authorization Middleware
// Required to process the login/logout requests
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();