using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LookingForController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public LookingForController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<LookingFor>>> GetLookingFors(string name)
        {
            if (name == null) return BadRequest("Please input name");
            return Ok(await _uow.LookingForRepository.SearchLookingFors(name));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<LookingFor>> CreateLookingFor(LookingFor lF)
        {
            if (lF.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.LookingForRepository.GetLookingForByName(lF.Name);

            if (checkname != null) return BadRequest("This name has taken");

            _uow.LookingForRepository.AddLookingFor(lF);

            if (await _uow.Complete()) return Ok("Created successfully.");

            return BadRequest("Failed to create. Please check your input and try again.");
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<LookingFor>> UpdateLookingFor(LookingFor lF, int id)
        {
            if (lF.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.LookingForRepository.GetLookingForByName(lF.Name);

            if (checkname != null) return BadRequest("This name has taken");

            lF.Id = id;

            _uow.LookingForRepository.UpdateLookingFor(lF);

            if (await _uow.Complete()) return Ok("Updated successfully.");

            return BadRequest("Unable to update. Please check your input and try again.");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<LookingFor>> DeleteLookingFor(int id)
        {
            if (_uow.LookingForRepository.DeleteLookingFor(id) == false) return NotFound();

            if (await _uow.Complete()) return Ok("Deleted successfully");

            return BadRequest("Unable to delete. Please try again later.");
        }
    }
}