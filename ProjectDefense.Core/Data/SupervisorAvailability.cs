using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Core.Data
{
    public class SupervisorAvailability
    {
        public int Id { get; set; }
        
        [Required]
        public string SupervisorId { get; set; }
        public ApplicationUser Supervisor { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a room.")]
        [Display(Name = "Room")]
        public int RoomId { get; set; }
        public Room Room { get; set; }
        
        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Time")]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(5, 120, ErrorMessage = "Slot duration must be between 5 and 120 minutes.")]
        [Display(Name = "Slot Duration (minutes)")]
        public int SlotDurationInMinutes { get; set; }
    }
}
