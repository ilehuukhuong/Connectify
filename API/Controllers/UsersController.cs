using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public UsersController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUser = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            userParams.CurrentUsername = currentUser.UserName;
            userParams.CurrentUserId = currentUser.Id;

            if (userParams.CurrentLatitude == null || userParams.CurrentLongitude == null)
            {
                userParams.CurrentLatitude = currentUser.Latitude;
                userParams.CurrentLongitude = currentUser.Longitude;
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);

            foreach (var user in users)
            {
                user.Distance = (int)CoordinateExtensions.CalculateDistance(userParams.CurrentLatitude.Value, userParams.CurrentLongitude.Value, user.Latitude, user.Longitude);
            }

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]

        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            if (user.UserName == username.ToLower())
            {
                return _mapper.Map<MemberDto>(user);
            }

            var targetUser = await _uow.UserRepository.GetUserByUsernameAsync(username.ToLower());

            if (targetUser == null) return NotFound();

            if (targetUser.IsBlocked || targetUser.IsDeleted) NotFound();

            if (await _uow.LikesRepository.GetUserLike(targetUser.Id, user.Id) == null && targetUser.IsVisible == false) return NotFound();

            return _mapper.Map<MemberDto>(targetUser);
        }

        [HttpPut("update-location")]
        public async Task<ActionResult> UpdateLocation(LocationDto locationDto)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();
            if (user.Latitude == locationDto.Latitude || user.Longitude == locationDto.Longitude) return NoContent();

            _mapper.Map(locationDto, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update location");
        }

        [HttpPut("visible")]
        public async Task<ActionResult> UpdateVisible()
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user != null)
            {
                user.IsVisible = true;
                if (await _uow.Complete()) return NoContent();
            }
            return NotFound("User not found");
        }

        [HttpPut("invisible")]
        public async Task<ActionResult> UpdateInvisible()
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user != null)
            {
                user.IsVisible = false;
                if (await _uow.Complete()) return NoContent();
            }
            return NotFound("User not found");
        }

        [HttpPut("delete-account")]
        public async Task<ActionResult> DeleteAccount()
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user != null)
            {
                user.IsDeleted = true;
                if (await _uow.Complete()) return Ok("Account deleted");
            }
            return NotFound("User not found");
        }
    }
}