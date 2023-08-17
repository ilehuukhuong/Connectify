using API.DTOs;
using API.Entities;
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
        public async Task<ActionResult<PagedList<MemberDtoWithoutIsVisible>>> GetUsers([FromQuery] UserParams userParams)
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
        public async Task<ActionResult<object>> GetUser(string username)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            if (user.UserName == username.ToLower())
            {
                return _mapper.Map<MemberDto>(user);
            }

            var targetUser = await _uow.UserRepository.GetUserByUsernameAsync(username.ToLower());

            if (targetUser == null) return NotFound();

            if (targetUser.IsBlocked || targetUser.IsDeleted) return NotFound();

            if (await _uow.LikesRepository.GetUserLike(targetUser.Id, user.Id) == null && targetUser.IsVisible == false) return NotFound();

            var returnUser = _mapper.Map<MemberDtoWithoutIsVisible>(targetUser);

            returnUser.Distance = (int)CoordinateExtensions.CalculateDistance(user.Latitude, user.Longitude, targetUser.Latitude, targetUser.Longitude);

            return returnUser;
        }

        [HttpPost("add-lookingfor/{id}")]
        public async Task<ActionResult> AddLookingFor(int id)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var lookingFor = await _uow.LookingForRepository.GetLookingForById(id);
            if (lookingFor == null) return NotFound();

            if (user.UserLookingFors.Count >= 3) return BadRequest("You can only have 3 looking fors");

            if (await _uow.UserRepository.GetUserLookingForEntityAsync(user, lookingFor) != null) return BadRequest("You already have this looking for");

            var userLookingFor = new UserLookingFor{
                User = user,
                LookingFor = lookingFor
            };

            user.UserLookingFors.Add(userLookingFor);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to add looking for");
        }

        [HttpDelete("delete-lookingfor/{id}")]
        public async Task<ActionResult> DeleteLookingFor(int id)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var lookingFor = await _uow.LookingForRepository.GetLookingForById(id);
            if (lookingFor == null) return NotFound();

            var userLookingFor = await _uow.UserRepository.GetUserLookingForEntityAsync(user, lookingFor);

            if (userLookingFor == null) return NotFound();

            user.UserLookingFors.Remove(userLookingFor);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to delete looking for");
        }

        [HttpPost("add-interest/{id}")]
        public async Task<ActionResult> AddInterest(int id)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var interest = await _uow.InterestRepository.GetInterestById(id);
            if (interest == null) return NotFound();

            if (user.UserInterests.Count >= 5) return BadRequest("You can only have 5 interests");

            if (await _uow.UserRepository.GetUserInterestForEntityAsync(user, interest) != null) return BadRequest("You already have this interest");

            var userInterest = new UserInterest{
                User = user,
                Interest = interest
            };

            user.UserInterests.Add(userInterest);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to add interest");
        }

        [HttpDelete("delete-interest/{id}")]
        public async Task<ActionResult> DeleteInterest(int id)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var interest = await _uow.InterestRepository.GetInterestById(id);
            if (interest == null) return NotFound();

            var userInterest = await _uow.UserRepository.GetUserInterestForEntityAsync(user, interest);

            if (userInterest == null) return NotFound();

            user.UserInterests.Remove(userInterest);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to delete interest");
        }

        [HttpPut("update-location")]
        public async Task<ActionResult> UpdateLocation(LocationDto locationDto)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();
            if (user.Latitude == locationDto.Latitude && user.Longitude == locationDto.Longitude) return NoContent();

            var checkCity = await _uow.CityRepository.GetCityByName(locationDto.LocationName);
            var tempCity = -1;

            if (checkCity == null)
            {
                var city = new City
                {
                    Name = locationDto.LocationName
                };

                _uow.CityRepository.AddCity(city);

                if (user.CityId != null) tempCity = user.CityId.Value;

                user.City = city;
            }
            else
            {
                if (user.CityId != null) tempCity = user.CityId.Value;
                user.City = checkCity;
            }

            _mapper.Map(locationDto, user);

            if (await _uow.Complete())
            {
                if (tempCity != -1)
                {
                    if (_uow.CityRepository.DeleteCity(tempCity))
                    {
                        if (await _uow.Complete()) return NoContent();
                        return BadRequest("Failed to delete city");
                    }
                }
                return NoContent();
            }
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
            return BadRequest("Failed to update visible");
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
            return BadRequest("Failed to update invisible");
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

        [HttpGet("recommended")]
        public async Task<ActionResult<PagedList<MemberDtoWithoutIsVisible>>> GetRecommendedMembers([FromQuery] UserParams userParams)
        {
            var currentUser = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var users = await _uow.UserRepository.GetRecommendedMembersAsync(currentUser, userParams);

            foreach (var user in users)
            {
                user.Distance = (int)CoordinateExtensions.CalculateDistance(currentUser.Latitude, currentUser.Longitude, user.Latitude, user.Longitude);
            }

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}