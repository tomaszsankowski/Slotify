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
    public class UnblockPeriodModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UnblockPeriodModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Start of unblock")]
            public DateTime StartTime { get; set; } = DateTime.Now;

            [Required]
            [Display(Name = "End of unblock")]
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

            var reservationsToUnblock = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                .Where(r => r.SupervisorAvailability.SupervisorId == user.Id &&
                            r.IsBlocked &&
                            r.StartTime < Input.EndTime && r.EndTime > Input.StartTime)
                .ToListAsync();

            foreach (var reservation in reservationsToUnblock)
            {
                reservation.IsBlocked = false;
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"{reservationsToUnblock.Count} slots have been unblocked in the selected period.";

            return RedirectToPage("/Supervisor/Reservations/Index");
        }
    }
}