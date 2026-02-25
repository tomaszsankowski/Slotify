using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectDefense.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Supervisor
{
    [Authorize(Roles = "Supervisor")]
    public class StudentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<ApplicationUser> Students { get; set; }
        public IList<string> BlockedStudents { get; set; }

        public async Task OnGetAsync()
        {
            Students = await _userManager.GetUsersInRoleAsync("Student");
            var lockedOutUsers = await _context.Users
                .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow)
                .Select(u => u.Id)
                .ToListAsync();
            BlockedStudents = new List<string>(lockedOutUsers);
        }

        public async Task<IActionResult> OnPostBanAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.StudentId == id);
                if (reservation != null)
                {
                    reservation.StudentId = null;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToPage();
        }
    }
}
