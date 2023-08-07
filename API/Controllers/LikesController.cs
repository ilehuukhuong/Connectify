using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(User.GetUserId());

            if (likedUser == null || sourceUser == null) return NotFound();

            if (sourceUser.UserName == likedUser.UserName) return BadRequest("You cannot like yourself");

            if (likedUser.IsBlocked || likedUser.IsDeleted) return NotFound();

            if (await _uow.LikesRepository.GetUserLike(likedUser.Id, sourceUser.Id) == null && likedUser.IsVisible == false) return NotFound();

            var userLike = await _uow.LikesRepository.GetUserLike(sourceUser.Id, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUser.Id,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, 
                users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}