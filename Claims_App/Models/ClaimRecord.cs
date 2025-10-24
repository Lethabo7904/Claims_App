using System.ComponentModel.DataAnnotations;
using ClaimsApp.Models;

namespace ClaimsApp.Models
{
    public class ClaimRecord
    {
        public int Id { get; set; }

        [Required]
        public string LecturerName { get; set; } = "";

        [Required]
        [Range(0.5, 1000)]
        public double HoursWorked { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal HourlyRate { get; set; }

        public string? Notes { get; set; }

        public string? UploadedFilePath { get; set; }  // relative path under wwwroot/uploads

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    }
}
