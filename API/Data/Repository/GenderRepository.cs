using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class GenderRepository : IGenderRepository
    {
        private readonly DataContext _context;
        public GenderRepository(DataContext context)
        {
            _context = context;
        }
        public void AddGender(Gender gender)
        {
            _context.Genders.Add(gender);
        }

        public void UpdateGender(Gender gender)
        {
            _context.Genders.Update(gender);
        }

        public bool DeleteGender(int id)
        {
            if (_context.Users.FirstOrDefault(x => x.GenderId == id) != null) return false;

            _context.Genders.Remove(_context.Genders.FirstOrDefault(x => x.Id == id));

            return true;
        }

        public async Task<Gender> GetGenderById(int id)
        {
            return await _context.Genders.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Gender> GetGenderByName(string name)
        {
            return await _context.Genders.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Gender>> GetGenders()
        {
            return await _context.Genders.OrderBy(x => x.Name).ToListAsync();
        }
    }
}