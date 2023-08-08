using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class CityRepository : ICityRepository
    {
        private readonly DataContext _context;
        public CityRepository(DataContext context)
        {
            _context = context;
        }

        public void AddCity(City lF)
        {
            _context.Cities.Add(lF);
        }

        public bool DeleteCity(int id)
        {
            var lF = _context.Cities.FirstOrDefault(x => x.Id == id);

            if (lF == null) return false;

            _context.Cities.Remove(lF);

            return true;
        }

        public async Task<City> GetCityById(int id)
        {
            return await _context.Cities.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<City> GetCityByName(string name)
        {
            return await _context.Cities.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<City>> SearchCities(string name)
        {
            return await _context.Cities
                .Where(x => x.Name.ToLower()
                .Contains(name.ToLower()))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public void UpdateCity(City lF)
        {
            _context.Cities.Update(lF);
        }
    }
}