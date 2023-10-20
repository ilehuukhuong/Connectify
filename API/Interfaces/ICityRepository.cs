using API.Entities;

namespace API.Interfaces
{
    public interface ICityRepository
    {
        void AddCity(City lF);
        bool DeleteCity(int id);
        void UpdateCity(City lF);
        Task<City> GetCityById(int id);
        Task<City> GetCityByName(string name);
        Task<IEnumerable<City>> SearchCities(string name);
    }
}