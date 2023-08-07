using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class LookingForRepository : ILookingForRepository
    {
        private readonly DataContext _context;
        public LookingForRepository(DataContext context)
        {
            _context = context;
        }

        public void AddLookingFor(LookingFor lF)
        {
            _context.LookingFors.Add(lF);
        }

        public bool DeleteLookingFor(int id)
        {
            var lF = _context.LookingFors.FirstOrDefault(x => x.Id == id);

            if (lF == null) return false;

            _context.LookingFors.Remove(lF);

            return true;
        }

        public async Task<LookingFor> GetLookingForById(int id)
        {
            return await _context.LookingFors.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LookingFor> GetLookingForByName(string name)
        {
            return await _context.LookingFors.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<LookingFor>> GetLookingFors(string name)
        {
            return await _context.LookingFors.Where(x => x.Name.ToLower() == name.ToLower()).OrderBy(x => x.Name).ToListAsync();
        }

        public void UpdateLookingFor(LookingFor lF)
        {
            _context.LookingFors.Update(lF);
        }
    }
}