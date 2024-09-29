using Infrastructure;

namespace BookingDriverAss1.Middlware
{
    public class ConfirmationTokenMiddleware
    {
        private readonly RequestDelegate _next;
        public ConfirmationTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // create area service temporary
            using (var scope = context.RequestServices.CreateScope())
            {
                // Get the UnitOfWork from the temporary service scope
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                var token = context.Request.Query["token"];

                if (!string.IsNullOrEmpty(token))
                {
                    var user = await unitOfWork.UserRepository.GetUserByConfirmationToken(token);

                    if (user != null && !user.IsConfirm)
                    {
                        // confirm
                        user.IsConfirm = true;
                        user.Token = null;
                        await unitOfWork.SaveChangeAsync();
                        context.Response.Redirect("https://localhost:7128/swagger/index.html");                   
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
