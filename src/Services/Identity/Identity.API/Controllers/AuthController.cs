namespace Identity.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    LoginHandler loginHandler,
    LogoutHandler logoutHandler) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await loginHandler.Handle(
            new LoginCommand(request.Email, request.Password), ct);

        return result.Match<IActionResult>(
            onSuccess: token => Ok(token),
            onFailure: error => error.Code switch
            {
                "identity.credentials.invalid" => Unauthorized(error.ToResponse()),
                "identity.user.inactive"        => Forbid(),
                _                               => BadRequest(error.ToResponse())
            });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = HttpContext.Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty);

        var result = await logoutHandler.Handle(new LogoutCommand(token), ct);

        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => BadRequest(error.ToResponse()));
    }
}

public sealed record LoginRequest(string Email, string Password);