namespace API.Entities
{
    public class UserLookingFor
    {
        public AppUser User { get; set; }
        public int UserId { get; set; }
        public LookingFor LookingFor { get; set; }
        public int LookingForId { get; set; }
    }
}