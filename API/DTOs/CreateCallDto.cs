namespace API.DTOs
{
    public class CreateCallDto
    {
        public string CallerUsername { get; set; }
        public string RecipientUsername { get; set; }
        public bool IsVoiceEnabled { get; set; }
        public bool IsVideoEnabled { get; set; }
        public bool IsScreenSharingEnabled { get; set; }
    }
}