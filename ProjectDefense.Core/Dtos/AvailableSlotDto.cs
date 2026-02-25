namespace ProjectDefense.Core.Dtos
{
    public class AvailableSlotDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RoomName { get; set; }
        public string SupervisorName { get; set; }
    }
}