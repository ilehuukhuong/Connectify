using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class InterestController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public InterestController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        [HttpGet("{name}")]
        public async Task<ActionResult<IEnumerable<Interest>>> GetInterests(string name)
        {
            if (name == null) return BadRequest("Please input name");
            return Ok(await _uow.InterestRepository.SearchInterests(name));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<Interest>> CreateInterest(Interest lF)
        {
            if (lF.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.InterestRepository.GetInterestByName(lF.Name);

            if(checkname != null) return BadRequest("This name has taken");

            _uow.InterestRepository.AddInterest(lF);

            if (await _uow.Complete()) return Ok("Created successfully.");

            return BadRequest("Failed to create. Please check your input and try again.");
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<Interest>> UpdateInterest(Interest lF, int id)
        {
            if (lF.Name == null) return BadRequest("Name is required");

            var checkname = await _uow.InterestRepository.GetInterestByName(lF.Name);

            if(checkname != null) return BadRequest("This name has taken");

            lF.Id = id;

            _uow.InterestRepository.UpdateInterest(lF);

            if (await _uow.Complete()) return Ok("Updated successfully.");

            return BadRequest("Unable to update. Please check your input and try again.");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<Interest>> DeleteInterest(int id)
        {
            if(_uow.InterestRepository.DeleteInterest(id) == false) return NotFound();

            if (await _uow.Complete()) return Ok("Deleted successfully");

            return BadRequest("Unable to delete. Please try again later.");
        }
    }
}