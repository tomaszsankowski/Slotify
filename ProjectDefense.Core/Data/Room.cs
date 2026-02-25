using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Core.Data
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Room Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; }
    }
}
