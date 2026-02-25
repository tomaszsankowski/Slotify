using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Reservations
{
    [Authorize(Roles = "Supervisor")]
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

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Reservation = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                    .ThenInclude(s => s.Room)
                .Include(r => r.Student)
                .Where(r => r.SupervisorAvailability.SupervisorId == user.Id)
                .OrderBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.IsBlocked = true;
                if (reservation.StudentId != null)
                {
                    reservation.StudentId = null;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.IsBlocked = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
