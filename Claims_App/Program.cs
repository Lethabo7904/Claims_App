using ClaimsApp.Data;
using ClaimsApp.Hubs;
using ClaimsApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// ✅ Use in-memory database (no SQLite needed)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ClaimsDB"));

var app = builder.Build();

// ✅ Ensure DB created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Claims.Any())
    {
        db.Claims.AddRange(
            new ClaimRecord
            {
                LecturerName = "Lethabo Maphoto",
                HoursWorked = 8,
                HourlyRate = 300,
                Status = ClaimStatus.Pending,
                DateSubmitted = DateTime.UtcNow.AddDays(-1)
            },
            new ClaimRecord
            {
                LecturerName = "Morongwa Thokollo",
                HoursWorked = 5,
                HourlyRate = 250,
                Status = ClaimStatus.Approved,
                DateSubmitted = DateTime.UtcNow.AddDays(-5)
            },
            new ClaimRecord
            {
                LecturerName = "Ntombikayise Mkhonto",
                HoursWorked = 8,
                HourlyRate = 300,
                Status = ClaimStatus.Rejected,
                DateSubmitted = DateTime.UtcNow.AddDays(-10)
            }
        );

        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Claims}/{action=Index}/{id?}");

app.MapHub<ClaimsHub>("/claimshub");

app.Run();

