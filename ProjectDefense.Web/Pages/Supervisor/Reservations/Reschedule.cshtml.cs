using System.ComponentModel.DataAnnotations;
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
    public class RescheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RescheduleModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Reservation CurrentReservation { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a new time slot.")]
        [Display(Name = "New Time Slot")]
        public int NewReservationId { get; set; }

        public IList<Reservation> AvailableSlots { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentReservation = await _context.Reservations
                .Include(r => r.Student)
                .Include(r => r.SupervisorAvailability.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (CurrentReservation == null || CurrentReservation.StudentId == null)
            {
                return NotFound("This reservation cannot be rescheduled because it is not assigned to a student.");
            }

            var user = await _userManager.GetUserAsync(User);
            AvailableSlots = await _context.Reservations
                .Where(r => r.SupervisorAvailability.SupervisorId == user.Id &&
                            r.StudentId == null &&
                            !r.IsBlocked &&
                            r.StartTime > DateTime.Now)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ModelState.Remove(nameof(CurrentReservation.SupervisorAvailability));
            ModelState.Remove(nameof(CurrentReservation.Student));
            ModelState.Remove(nameof(CurrentReservation.StudentId));

            if (!ModelState.IsValid)
            {
                await OnGetAsync(id);
                return Page();
            }

            var originalReservation = await _context.Reservations.FindAsync(id);
            var newReservation = await _context.Reservations.FindAsync(NewReservationId);

            if (originalReservation == null || newReservation == null)
            {
                return NotFound();
            }

            if (newReservation.StudentId != null || newReservation.IsBlocked)
            {
                TempData["ErrorMessage"] = "The selected new time slot is no longer available.";
                return RedirectToPage("./Index");
            }

            newReservation.StudentId = originalReservation.StudentId;
            originalReservation.StudentId = null;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "The student has been successfully rescheduled.";
            return RedirectToPage("./Index");
        }
    }
}