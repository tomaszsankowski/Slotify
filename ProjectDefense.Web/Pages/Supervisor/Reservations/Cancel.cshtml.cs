using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Reservations
{
    [Authorize(Roles = "Supervisor")]
    public class CancelModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;

        public CancelModel(Data.ApplicationDbContext context)
        {
            _context = context;
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
                .Include(r => r.Student).FirstOrDefaultAsync(m => m.Id == id);

            if (Reservation == null)
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

            Reservation = await _context.Reservations.FindAsync(id);

            if (Reservation != null)
            {
                Reservation.StudentId = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
