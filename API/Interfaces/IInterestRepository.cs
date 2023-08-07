using API.Entities;

namespace API.Interfaces
{
    public interface IInterestRepository
    {
        void AddInterest(Interest lF);
        bool DeleteInterest(int id);
        void UpdateInterest(Interest lF);
        Task<Interest> GetInterestById(int id);
        Task<Interest> GetInterestByName(string name);
        Task<IEnumerable<Interest>> GetInterests (string name);
    }
}