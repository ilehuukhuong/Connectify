namespace API.DTOs
{
    public class MemberUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public int GenderId { get; set; }
    }
}