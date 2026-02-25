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
    public class BookModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Reservation Reservation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Reservation = await _context.Reservations
                .Include(r => r.SupervisorAvailability)
                .ThenInclude(s => s.Room)
                .Include(r => r.SupervisorAvailability)
                .ThenInclude(s => s.Supervisor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Reservation == null || Reservation.IsBlocked)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Your account is blocked. You cannot make reservations.");
                await OnGetAsync(id);
                return Page();
            }

            var hasReservation = await _context.Reservations.AnyAsync(r => r.StudentId == user.Id);

            if (hasReservation)
            {
                return RedirectToPage("./MyReservation");
            }

            Reservation = await _context.Reservations.FindAsync(id);

            if (Reservation != null && Reservation.StudentId == null && !Reservation.IsBlocked)
            {
                Reservation.StudentId = user.Id;
                await _context.SaveChangesAsync();
                return RedirectToPage("./MyReservation");
            }

            TempData["ErrorMessage"] = "This time slot is no longer available.";
            return RedirectToPage("./Index");
        }
    }
}
