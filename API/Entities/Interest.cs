namespace API.Entities
{
    public class Interest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserInterest> UserInterests { get; set; }
    }
}