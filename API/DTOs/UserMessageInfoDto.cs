namespace API.DTOs
{
    public class UserMessageInfoDto
    {
        public string PhotoUrl { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTime MessageSent { get; set; }
    }
}