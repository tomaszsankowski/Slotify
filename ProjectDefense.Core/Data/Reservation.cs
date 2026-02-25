using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Core.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public int SupervisorAvailabilityId { get; set; }
        public SupervisorAvailability SupervisorAvailability { get; set; }

        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
        public bool IsBlocked { get; set; } = false;
    }
}
