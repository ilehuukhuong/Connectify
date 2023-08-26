namespace API.Entities
{
    public class Call
    {
        public int Id { get; set; }
        public string CallerUsername { get; set; }
        public AppUser Caller { get; set; }
        public string RecipientUsername { get; set; }
        public AppUser Receiver { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
    }
}