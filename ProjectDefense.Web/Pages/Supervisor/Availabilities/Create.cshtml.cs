using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Availabilities
{
    [Authorize(Roles = "Supervisor")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public SupervisorAvailability SupervisorAvailability { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            SupervisorAvailability.SupervisorId = user.Id;

            ModelState.Remove("SupervisorAvailability.Supervisor");
            ModelState.Remove("SupervisorAvailability.SupervisorId");
            ModelState.Remove("SupervisorAvailability.Room");

            if (SupervisorAvailability.EndDate <= SupervisorAvailability.StartDate)
            {
                ModelState.AddModelError("SupervisorAvailability.EndDate", "End date must be after the start date.");
            }

            var conflictingAvailability = await _context.SupervisorAvailabilities
                .AnyAsync(a => a.SupervisorId == user.Id &&
                               a.RoomId == SupervisorAvailability.RoomId &&
                               a.StartDate < SupervisorAvailability.EndDate &&
                               a.EndDate > SupervisorAvailability.StartDate);

            if (conflictingAvailability)
            {
                ModelState.AddModelError(string.Empty, "The new availability conflicts with an existing one in the same room and time.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
                return Page();
            }

            SupervisorAvailability.Room = await _context.Rooms.FindAsync(SupervisorAvailability.RoomId);

            _context.SupervisorAvailabilities.Add(SupervisorAvailability);
            await _context.SaveChangesAsync();

            GenerateSlots(SupervisorAvailability);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void GenerateSlots(SupervisorAvailability availability)
        {
            var currentDateTime = availability.StartDate;
            while (currentDateTime.AddMinutes(availability.SlotDurationInMinutes) <= availability.EndDate)
            {
                var slot = new Reservation
                {
                    SupervisorAvailabilityId = availability.Id,
                    StartTime = currentDateTime,
                    EndTime = currentDateTime.AddMinutes(availability.SlotDurationInMinutes)
                };
                _context.Reservations.Add(slot);
                currentDateTime = currentDateTime.AddMinutes(availability.SlotDurationInMinutes);
            }
        }
    }
}
