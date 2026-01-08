using SoSuSaFsd.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoSuSaFsd.Data;
using SoSuSaFsd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SoSuSaFsd.Domain;

var builder = WebApplication.CreateBuilder(args);

// ========== DATABASE ==========
builder.Services.AddDbContextFactory<SoSuSaFsdContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoSuSaFsdContext") ?? 
        throw new InvalidOperationException("Connection string 'SoSuSaFsdContext' not found.")));
        
builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ========== SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ========== SIGNALR ==========
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 100; // 100MB
});

// ========== IDENTITY ==========
builder.Services.AddIdentityCore<Users>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
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

// ========== AUTHENTICATION ==========
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddCascadingAuthenticationState();

// ========== BLAZOR ==========
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ========== DATABASE SEEDING ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure roles exist
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create or reset admin user
        string targetEmail = "So@gmail.com";
        string targetUsername = "SoSuSaAdmin";
        string targetPassword = "SoSoSaAdmin123";

        var adminUser = await userManager.FindByEmailAsync(targetEmail);

        if (adminUser == null)
        {
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
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, targetPassword);

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

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
        logger.LogError(ex, "Error seeding database.");
    }
}

// ========== MIDDLEWARE ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();