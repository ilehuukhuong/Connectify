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
                
                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: The account was not found.");
                    return;
                }

                if (user.IsBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Your account has been blocked.");
                    return;
                }

                if (user.IsDeleted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Your account has been deleted.");
                    return;
                }
            }

            await _next(context);
        }
    }
}