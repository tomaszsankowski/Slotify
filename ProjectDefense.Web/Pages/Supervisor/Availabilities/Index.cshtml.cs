using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Web.Data;

namespace ProjectDefense.Web.Pages.Supervisor.Availabilities
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

        public IList<SupervisorAvailability> SupervisorAvailability { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            SupervisorAvailability = await _context.SupervisorAvailabilities
                .Include(s => s.Room)
                .Where(s => s.SupervisorId == user.Id)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }
    }
}
