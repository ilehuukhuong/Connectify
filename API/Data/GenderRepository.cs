using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
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

        public void DeleteGender(Gender gender)
        {
            _context.Genders.Remove(gender);
        }

        public async Task<Gender> GetGenderById(int id)
        {
            return await _context.Genders.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Gender> GetGenderByName(string name)
        {
            return await _context.Genders.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<IEnumerable<Gender>> GetGenders()
        {
            return await _context.Genders.ToListAsync();
        }
    }
}