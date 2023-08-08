using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class InterestRepository : IInterestRepository
    {
        private readonly DataContext _context;
        public InterestRepository(DataContext context)
        {
            _context = context;
        }

        public void AddInterest(Interest lF)
        {
            _context.Interests.Add(lF);
        }

        public bool DeleteInterest(int id)
        {
            var lF = _context.Interests.FirstOrDefault(x => x.Id == id);

            if (lF == null) return false;

            _context.Interests.Remove(lF);

            return true;
        }

        public async Task<Interest> GetInterestById(int id)
        {
            return await _context.Interests.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Interest> GetInterestByName(string name)
        {
            return await _context.Interests.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Interest>> SearchInterests(string name)
        {
            return await _context.Interests
                .Where(x => x.Name.ToLower()
                .Contains(name.ToLower()))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public void UpdateInterest(Interest lF)
        {
            _context.Interests.Update(lF);
        }
    }
}