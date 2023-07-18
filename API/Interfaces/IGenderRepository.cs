using API.Entities;

namespace API.Interfaces
{
    public interface IGenderRepository
    {
        void AddGender(Gender gender);
        bool DeleteGender(int id);
        void UpdateGender(Gender gender);
        Task<Gender> GetGenderById(int id);
        Task<Gender> GetGenderByName(string name);
        Task<IEnumerable<Gender>> GetGenders ();
    }
}