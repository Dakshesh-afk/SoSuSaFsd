using SoSuSaFsd.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoSuSaFsd.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SoSuSaFsd.Domain;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext Configuration
builder.Services.AddDbContextFactory<SoSuSaFsdContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoSuSaFsdContext") ?? throw new InvalidOperationException("Connection string 'SoSuSaFsdContext' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- CRITICAL FIX 1: Add Controller Services ---
builder.Services.AddControllers();

// --- CRITICAL FIX 4: INCREASE SIGNALR MESSAGE LIMIT ---
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // 10MB
});

// 2. Add Identity Services
builder.Services.AddIdentityCore<Users>(options =>
{
    // Login settings
    options.SignIn.RequireConfirmedAccount = false;

    // Password settings
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

// --- NEW ADMIN SEEDING LOGIC ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Ensure Roles Exist
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Define the SPECIFIC Admin credentials you requested
        string targetEmail = "So@gmail.com";
        string targetUsername = "SoSuSaAdmin";
        string targetPassword = "SoSoSaAdmin123";

        // Check if this specific user exists
        var adminUser = await userManager.FindByEmailAsync(targetEmail);

        if (adminUser == null)
        {
            // CREATE NEW ADMIN
            adminUser = new Users
            {
                UserName = targetUsername,
                Email = targetEmail,
                FirstName = "Super",
                LastName = "Admin",
                DisplayName = "SoSuSa Admin",
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                DateOfBirth = DateTime.Now,
                IsActive = true,
                IsVerified = true,
                Role = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, targetPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // IF EXISTS: FORCE RESET PASSWORD to 'SoSoSaAdmin123'
            // This fixes "Invalid Login" if the account was made previously with a different password
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, targetPassword);

            // Force Role Assignment
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Fix Database Role Column
            if (adminUser.Role != "Admin")
            {
                adminUser.Role = "Admin";
                await userManager.UpdateAsync(adminUser);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// --- END SEEDING LOGIC ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// 4. Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();