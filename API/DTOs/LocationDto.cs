using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LocationDto
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public string LocationName { get; set; }
    }
}