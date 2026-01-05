using SoSuSaFsd.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoSuSaFsd.Data;
using Microsoft.AspNetCore.Identity; // Required for IdentityRole
using Microsoft.AspNetCore.SignalR; // Required for HubOptions

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext Configuration
builder.Services.AddDbContextFactory<SoSuSaFsdContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoSuSaFsdContext") ?? throw new InvalidOperationException("Connection string 'SoSuSaFsdContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- CRITICAL FIX 1: Add Controller Services ---
builder.Services.AddControllers();

// --- CRITICAL FIX 4: INCREASE SIGNALR MESSAGE LIMIT ---
// This prevents the app from crashing when you try to preview/upload images larger than 32KB
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // Set limit to 10MB
});

// 2. Add Identity Services
builder.Services.AddIdentityCore<SoSuSaFsd.Domain.Users>(options =>
{
    // --- CRITICAL FIX 2: Allow login without email confirmation ---
    options.SignIn.RequireConfirmedAccount = false;

    // --- RELAX PASSWORD REQUIREMENTS ---
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SoSuSaFsdContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// 3. Add Authentication State
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
app.UseAuthentication();
app.UseAuthorization();

// --- CRITICAL FIX 3: Map Controllers ---
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();