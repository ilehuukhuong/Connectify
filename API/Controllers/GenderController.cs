using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class GenderController : BaseApiController
    {
        private readonly DataContext _context;
        public GenderController(DataContext context)
        {
            _context = context;

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gender>>> GetGenders()
        {
            return Ok(await _context.Genders.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Gender>> CreateGender(Gender gender)
        {
            if (gender.Name == null) return BadRequest("Name is required");

            var checkname = await _context.Genders.FirstOrDefaultAsync(x => x.Name == gender.Name);

            if(checkname != null) return BadRequest("This name has taken");

            _context.Genders.Add(gender);
            await _context.SaveChangesAsync();

            return Ok(gender);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Gender>> UpdateGender(Gender gender, int id)
        {
            if (gender.Name == null) return BadRequest("Name is required");

            var checkname = await _context.Genders.FirstOrDefaultAsync(x => x.Name == gender.Name);

            if(checkname != null) return BadRequest("This name has taken");

            gender.Id = id;
            _context.Genders.Update(gender);
            await _context.SaveChangesAsync();

            return Ok(gender);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Gender>> DeleteGender(int id)
        {
            var gender = await _context.Genders.FirstOrDefaultAsync(x => x.Id == id);
            _context.Genders.Remove(gender);
            await _context.SaveChangesAsync();

            return Ok(gender.Name + " has been deleted");
        }
    }
}