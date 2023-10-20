using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class GenderController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public GenderController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gender>>> GetGenders()
        {
            return Ok(await _uow.GenderRepository.GetGenders());
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult> CreateGender(Gender gender)
        {
            if (gender.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.GenderRepository.GetGenderByName(gender.Name);

            if (checkname != null) return BadRequest("This name has taken");

            _uow.GenderRepository.AddGender(gender);

            if (await _uow.Complete()) return Ok("The gender has been created successfully.");

            return BadRequest("Failed to create gender");
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult> UpdateGender(Gender gender, int id)
        {
            if (gender.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.GenderRepository.GetGenderByName(gender.Name);

            if (checkname != null) return BadRequest("This name has taken");

            gender.Id = id;

            _uow.GenderRepository.UpdateGender(gender);

            if (await _uow.Complete()) return Ok("The gender has been updated successfully.");

            return BadRequest("Unable to update the gender. Please check your input and try again.");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult> DeleteGender(int id)
        {
            if (_uow.GenderRepository.DeleteGender(id) == false) return BadRequest("This gender is being used by some users");

            if (await _uow.Complete()) return Ok("The gender has been deleted successfully");

            return BadRequest("Unable to delete the gender. Please try again later.");
        }
    }
}