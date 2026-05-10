using AttendanceSync.Infrastructure;
using AttendanceSync.Infrastructure.Clients;
using AttendanceSync.Infrastructure.Services;
using AttendanceSync.Worker.Jobs;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Connection String
// =====================================================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// =====================================================
// Entity Framework Core
// =====================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// =====================================================
// Hangfire
// =====================================================
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
    config.UseSimpleAssemblyNameTypeSerializer();
    config.UseRecommendedSerializerSettings();

    config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        PrepareSchemaIfNecessary = true,
        QueuePollInterval = TimeSpan.FromSeconds(15)
    });
});

builder.Services.AddHangfireServer();

// =====================================================
// Http Clients
// =====================================================
builder.Services.AddHttpClient<IHikvisionClient, HikvisionClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Hikvision:BaseUrl"]!);
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddHttpClient<IKayanClient, KayanClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Kayan:BaseUrl"]!);
    client.Timeout = TimeSpan.FromMinutes(2);
});

// =====================================================
// Application Services
// =====================================================
builder.Services.AddScoped<IIntegrationLogService, IntegrationLogService>();
builder.Services.AddScoped<IAttendanceSyncService, AttendanceSyncService>();
builder.Services.AddScoped<AttendanceSyncJob>();

// =====================================================
// Build Application
// =====================================================
var app = builder.Build();

// =====================================================
// Hangfire Dashboard
// URL: http://localhost:5000/hangfire
// =====================================================
app.UseHangfireDashboard("/hangfire");

// =====================================================
// Apply Migrations Automatically
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// =====================================================
// Register Recurring Job
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var configuration =
        scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var recurringJobManager =
        scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    var cronExpression =
        configuration["SyncSettings:CronExpression"]
        ?? Cron.Hourly();

    recurringJobManager.AddOrUpdate<AttendanceSyncJob>(
        "attendance-sync-job",
        job => job.RunAsync(),
        cronExpression);
}

// =====================================================
// Default Endpoint (Optional)
// =====================================================
app.MapGet("/", () =>
    "Attendance Sync Service is running. Open /hangfire to view dashboard.");

// =====================================================
// Run Application
// =====================================================
app.Run();