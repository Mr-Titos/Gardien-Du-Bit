using Microsoft.Identity.Web;

namespace api_gardienbit.Services
{
    public class UserIdentifierMiddleware(UserIdentifierService userIdentifierService) : IMiddleware
    {
        private readonly UserIdentifierService _userIdentifierService = userIdentifierService;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if ((context.User.Identity?.IsAuthenticated ?? false) == false
                ||
                Guid.TryParse(context.User.GetObjectId(), out Guid externalUserId) == false)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                _userIdentifierService.CurrentInternalUser = await _userIdentifierService.GetInternalUserAsync(externalUserId, context.User.GetDisplayName() ?? throw new ArgumentNullException());
                await next(context);
            }
        }
    }
}
