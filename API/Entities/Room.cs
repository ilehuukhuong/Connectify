using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Room
    {
        public Room()
        {

        }

        public Room(string name)
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}