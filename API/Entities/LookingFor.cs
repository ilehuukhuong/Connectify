namespace API.Entities
{
    public class LookingFor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserLookingFor> UserLookingFors { get; set; }
    }
}