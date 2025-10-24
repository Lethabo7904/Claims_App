using ClaimsApp.Data;
using ClaimsApp.Models;
using ClaimsApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ClaimsApp.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ClaimsHub> _hub;

        private static readonly string[] AllowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
        private const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

        public ClaimsController(AppDbContext db, IWebHostEnvironment env, IHubContext<ClaimsHub> hub)
        {
            _db = db;
            _env = env;
            _hub = hub;
        }

        public IActionResult Index(string? role)
        {
            // role param simulates a simple permission check; real app should use Identity
            ViewBag.Role = role ?? "lecturer";
            string currentLecturer = "Current Lecturer"; // replace with identity name
            var myClaims = _db.Claims.Where(c => c.LecturerName == currentLecturer).OrderByDescending(c => c.DateSubmitted).ToList();
            ViewBag.MyClaims = myClaims;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(IFormCollection form, IFormFile? upload)
        {
            // Basic field parsing and validation
            string lecturer = "Current Lecturer"; // replace with logged in user
            if (!double.TryParse(form["HoursWorked"], out var hours))
            {
                TempData["Error"] = "Invalid hours value.";
                return RedirectToAction("Index");
            }
            if (!decimal.TryParse(form["HourlyRate"], out var rate))
            {
                TempData["Error"] = "Invalid rate value.";
                return RedirectToAction("Index");
            }

            var claim = new ClaimRecord
            {
                LecturerName = lecturer,
                HoursWorked = hours,
                HourlyRate = rate,
                Notes = form["Notes"]
            };

            if (upload != null && upload.Length > 0)
            {
                var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "File type not allowed.";
                    return RedirectToAction("Index");
                }
                if (upload.Length > MaxFileBytes)
                {
                    TempData["Error"] = "File too large (max 5 MB).";
                    return RedirectToAction("Index");
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                claim.UploadedFilePath = $"/uploads/{fileName}";
            }

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            // Notify admins via SignalR (optional)
            await _hub.Clients.All.SendAsync("ClaimSubmitted", claim.Id);

            TempData["Success"] = "Claim submitted successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Download(int id)
        {
            var claim = _db.Claims.Find(id);
            if (claim == null || string.IsNullOrEmpty(claim.UploadedFilePath)) return NotFound();
            var physicalPath = Path.Combine(_env.WebRootPath, claim.UploadedFilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(physicalPath)) return NotFound();
            var mime = "application/octet-stream";
            return PhysicalFile(physicalPath, mime, Path.GetFileName(physicalPath));
        }
    }
}

