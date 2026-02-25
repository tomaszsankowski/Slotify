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
    public class MyReservationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyReservationModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Reservation Reservation { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Reservation = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                    .ThenInclude(s => s.Room)
                .Include(r => r.SupervisorAvailability)
                    .ThenInclude(s => s.Supervisor)
                .FirstOrDefaultAsync(m => m.StudentId == user.Id);

            if (Reservation == null)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.StudentId == user.Id && r.Id == Reservation.Id);

            if (reservation != null)
            {
                if (reservation.StartTime < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "You cannot change a reservation that has already passed.";
                    return RedirectToPage();
                }
                reservation.StudentId = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostChangeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.StudentId == user.Id && r.Id == Reservation.Id);

            if (reservation != null)
            {
                if (reservation.StartTime < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "You cannot change a reservation that has already passed.";
                    return RedirectToPage();
                }
                reservation.StudentId = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}
