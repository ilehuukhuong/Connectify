using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Include(p => p.Photos)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList(),
                    isBlocked = u.IsBlocked,
                    isDeleted = u.IsDeleted,
                    PhotoUrl = u.Photos.FirstOrDefault(x => x.IsMain).Url
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("block/{username}")]
        public async Task<ActionResult> BlockUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username.ToLower());
            if (user != null)
            {
                user.IsBlocked = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded) return NoContent();
            }
            return NotFound("User not found");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("unblock/{username}")]
        public async Task<ActionResult> UnblockUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username.ToLower());
            if (user != null)
            {
                user.IsBlocked = false;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded) return NoContent();
            }
            return NotFound("User not found");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}