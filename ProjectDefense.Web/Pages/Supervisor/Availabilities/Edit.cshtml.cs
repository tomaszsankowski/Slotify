using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Availabilities
{
    [Authorize(Roles = "Supervisor")]
    public class EditModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;

        public EditModel(Data.ApplicationDbContext context)
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
           ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(SupervisorAvailability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupervisorAvailabilityExists(SupervisorAvailability.Id))
                {
                    return NotFound();
                }
            }

            return RedirectToPage("./Index");
        }

        private bool SupervisorAvailabilityExists(int id)
        {
            return _context.SupervisorAvailabilities.Any(e => e.Id == id);
        }
    }
}
