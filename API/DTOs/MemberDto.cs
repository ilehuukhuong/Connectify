namespace API.DTOs
{
    public class MemberDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string City { get; set; }
        public bool IsVisible { get; set; }
        public List<PhotoDto> Photos { get; set; }
        public List<IdNameDto> LookingFors { get; set; }
        public List<IdNameDto> Interests { get; set; }
    }
}