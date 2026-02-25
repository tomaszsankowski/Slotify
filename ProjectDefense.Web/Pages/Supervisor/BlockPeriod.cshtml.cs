using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Supervisor
{
    [Authorize(Roles = "Supervisor")]
    public class BlockPeriodModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlockPeriodModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Start of block")]
            public DateTime StartTime { get; set; } = DateTime.Now;

            [Required]
            [Display(Name = "End of block")]
            public DateTime EndTime { get; set; } = DateTime.Now.AddHours(1);
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            var reservationsToBlock = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                .Where(r => r.SupervisorAvailability.SupervisorId == user.Id &&
                            r.StartTime < Input.EndTime && r.EndTime > Input.StartTime)
                .ToListAsync();

            int canceledCount = 0;
            foreach (var reservation in reservationsToBlock)
            {
                reservation.IsBlocked = true;
                if (reservation.StudentId != null)
                {
                    reservation.StudentId = null;
                    canceledCount++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Period from {Input.StartTime} to {Input.EndTime} has been blocked. {reservationsToBlock.Count} slots were blocked and {canceledCount} student reservations were canceled.";

            return RedirectToPage("/Supervisor/Reservations/Index");
        }
    }
}