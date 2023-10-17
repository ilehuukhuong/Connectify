namespace API.Entities
{
    public class Call
    {
        public int Id { get; set; }
        public string CallerUsername { get; set; }
        public int CallerId { get; set; }
        public AppUser Caller { get; set; }
        public string ReceiverUsername { get; set; }
        public int ReceiverId { get; set; }
        public AppUser Receiver { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
    }
}