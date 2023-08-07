using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public DateOnly DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public int GenderId { get; set; }
        public Gender Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsBlocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsVisible  { get; set; } = true;
        public List<Photo> Photos { get; set; } = new();
        public List<UserLike> LikedByUsers { get; set; }
        public List<UserLike> LikedUsers { get; set; }
        public List<Message> MessagesSent { get; set; }
        public List<Message> MessagesReceived { get; set; }
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}