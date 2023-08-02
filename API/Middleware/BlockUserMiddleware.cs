using System.Security.Claims;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Middleware
{
    public class BlockUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserManager<AppUser> _userManager;

        public BlockUserMiddleware(RequestDelegate next, UserManager<AppUser> userManager)
        {
            _next = next;
            _userManager = userManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var username = context.User.GetUsername();
                var user = await _userManager.FindByNameAsync(username);
                if (user != null && user.IsBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: User is blocked");
                    return;
                }
            }

            await _next(context);
        }
    }
}