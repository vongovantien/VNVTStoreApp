using Newtonsoft.Json;
using System.Security.Claims;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure.Contexts;

namespace vnvt_back_end.API.Middlewares
{
    public class AdminMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await dbContext.Users.FindAsync(userId);
                if (user != null && user.Role == "Admin")
                {
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>(false, "Forbidden: You do not have access to this resource", null, StatusCodes.Status403Forbidden);
            var jsonResponse = JsonConvert.SerializeObject(response);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
