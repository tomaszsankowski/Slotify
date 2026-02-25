using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Student.Reservations
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Reservation> Reservation { get;set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var hasReservation = await _context.Reservations.AnyAsync(r => r.StudentId == user.Id);

            if (hasReservation)
            {
                return RedirectToPage("./MyReservation");
            }

            Reservation = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                    .ThenInclude(s => s.Room)
                .Include(r => r.SupervisorAvailability)
                    .ThenInclude(s => s.Supervisor)
                .Where(r => r.StudentId == null && r.StartTime > DateTime.Now)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            return Page();
        }
    }
}
