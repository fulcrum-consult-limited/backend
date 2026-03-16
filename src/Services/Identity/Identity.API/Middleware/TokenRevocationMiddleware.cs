namespace Identity.API.Middleware;

public sealed class TokenRevocationMiddleware(
    RequestDelegate next,
    ITokenRevocationStore revocationStore)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string jti;
            try
            {
                jti = context.User.GetJti();
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (await revocationStore.IsRevokedAsync(jti, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "token_revoked",
                    message = "This session has been revoked. Please log in again."
                });
                return;
            }
        }

        await next(context);
    }
}