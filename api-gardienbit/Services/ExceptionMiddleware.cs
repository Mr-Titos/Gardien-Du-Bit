using common_gardienbit.DTO.Exception;
using Microsoft.EntityFrameworkCore;

namespace api_gardienbit.Services
{
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (AbstractException ae)
            {
                context.Response.StatusCode = ae.statusCode;
                await context.Response.WriteAsJsonAsync(ae.Serialize());
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is DbUpdateConcurrencyException || ex is OperationCanceledException)
            {
                Console.WriteLine(ex);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "Database error", statusCode = context.Response.StatusCode });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "Internal server error", statusCode = context.Response.StatusCode });
            }
        }
    }
}
