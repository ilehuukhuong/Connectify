using System.Text.Json.Serialization;
using API.Entities;

namespace API.DTOs
{
    public class MemberDtoWithoutIsVisible
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PhotoUrl { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public int Distance { get; set; }
        public string Introduction { get; set; }
        public string City { get; set; }
        public List<PhotoDto> Photos { get; set; }
        public List<LookingFor> LookingFors { get; set; }
        public List<Interest> Interests {get; set; }
        [JsonIgnore]
        public double Latitude { get; set; }
        [JsonIgnore]
        public double Longitude { get; set; }
    }
}