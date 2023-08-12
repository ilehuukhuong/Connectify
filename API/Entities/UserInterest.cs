namespace API.Entities
{
    public class UserInterest
    {
        public AppUser User { get; set; }
        public int UserId { get; set; }
        public Interest Interest { get; set; }
        public int InterestId { get; set; }
    }
}