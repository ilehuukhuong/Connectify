using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class TermListsController : BaseApiController
    {
        private readonly IContentModeratorService _contentModeratorService;

        public TermListsController(IContentModeratorService contentModeratorService)
        {
            _contentModeratorService = contentModeratorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTermLists()
        {
            try
            {
                var result = await _contentModeratorService.GetAllTermListsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{listId}")]
        public async Task<IActionResult> GetAllTermsInTermList(string listId)
        {
            try
            {
                var result = await _contentModeratorService.GetAllTermsInTermListAsync(listId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTermList([FromBody] TermListCreateRequest request)
        {
            try
            {
                var responseBody = await _contentModeratorService.CreateTermListAsync(request.Name, request.Description);
                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("AddTerm/{listId}")]
        public async Task<IActionResult> AddTerm(string listId, [FromBody] TermRequest request)
        {
            try
            {
                await _contentModeratorService.AddTermToListAsync(listId, request.Term, request.Language);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteTerm/{listId}")]
        public async Task<IActionResult> DeleteTerm(string listId, [FromBody] TermRequest request)
        {
            try
            {
                await _contentModeratorService.DeleteTermFromListAsync(listId, request.Term, request.Language);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{listId}")]
        public async Task<IActionResult> DeleteTermList(string listId)
        {
            try
            {
                await _contentModeratorService.DeleteTermListAsync(listId);
                return Ok($"Term list with ID {listId} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{listId}")]
        public async Task<IActionResult> RefreshIndex(string listId)
        {
            try
            {
                await _contentModeratorService.RefreshTermListAsync(listId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}