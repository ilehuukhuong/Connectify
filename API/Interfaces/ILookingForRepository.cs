using API.Entities;

namespace API.Interfaces
{
    public interface ILookingForRepository
    {
        void AddLookingFor(LookingFor lF);
        bool DeleteLookingFor(int id);
        void UpdateLookingFor(LookingFor lF);
        Task<LookingFor> GetLookingForById(int id);
        Task<LookingFor> GetLookingForByName(string name);
        Task<IEnumerable<LookingFor>> SearchLookingFors (string name);
    }
}