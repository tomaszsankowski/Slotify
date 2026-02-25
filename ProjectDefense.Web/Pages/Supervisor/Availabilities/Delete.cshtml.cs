using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Availabilities
{
    [Authorize(Roles = "Supervisor")]
    public class DeleteModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;

        public DeleteModel(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SupervisorAvailability SupervisorAvailability { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SupervisorAvailability = await _context.SupervisorAvailabilities
                .Include(s => s.Room)
                .Include(s => s.Supervisor).FirstOrDefaultAsync(m => m.Id == id);

            if (SupervisorAvailability == null)
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

            SupervisorAvailability = await _context.SupervisorAvailabilities.FindAsync(id);

            if (SupervisorAvailability != null)
            {
                var reservations = _context.Reservations.Where(r => r.SupervisorAvailabilityId == id);
                _context.Reservations.RemoveRange(reservations);
                _context.SupervisorAvailabilities.Remove(SupervisorAvailability);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
