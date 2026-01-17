using SoSuSaFsd.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoSuSaFsd.Data;
using SoSuSaFsd.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SoSuSaFsd.Domain;
using Serilog;
using Serilog.Events;

// ========== CONFIGURE SERILOG ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/sosusa-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SoSuSaFsd application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

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
    builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

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
            var contextFactory = services.GetRequiredService<IDbContextFactory<SoSuSaFsdContext>>();
            using var context = contextFactory.CreateDbContext();
            
            var userManager = services.GetRequiredService<UserManager<Users>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

            // Create and run the seeder
            var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
            await seeder.SeedAsync();
            
            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    // ========== MIDDLEWARE ==========
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
        Log.Information("Running in Production mode");
    }
    else
    {
        app.UseMigrationsEndPoint();
        Log.Information("Running in Development mode");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}