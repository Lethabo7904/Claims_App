using Claims_App.Models;
using ClaimsApp.Data;
using ClaimsApp.Hubs;
using ClaimsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ClaimsApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ClaimsHub> _hub;

        public AdminController(AppDbContext db, IHubContext<ClaimsHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public IActionResult Index()
        {
            var pending = _db.Claims.OrderByDescending(c => c.DateSubmitted).ToList();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string action)
        {
            var claim = _db.Claims.Find(id);
            if (claim == null) return NotFound();

            if (action == "approve")
                claim.Status = ClaimStatus.Approved;
            else if (action == "reject")
                claim.Status = ClaimStatus.Rejected;
            else
                claim.Status = ClaimStatus.Pending;

            await _db.SaveChangesAsync();

            // Notify lecturers about status change
            await _hub.Clients.All.SendAsync("ClaimStatusChanged", claim.Id, claim.Status.ToString());

            return RedirectToAction("Index");
        }
    }
}

