using API.Extensions;
using API.Interfaces;

namespace API.Middleware
{
    public class BlockUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUnitOfWork _uow;

        public BlockUserMiddleware(RequestDelegate next, IUnitOfWork uow)
        {
            _uow = uow;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await _uow.UserRepository.BlockUserAsync(context.User.GetUserId());

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