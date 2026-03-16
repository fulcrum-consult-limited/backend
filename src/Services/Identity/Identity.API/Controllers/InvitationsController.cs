namespace Identity.API.Controllers;

[ApiController]
[Route("api/invitations")]
public sealed class InvitationsController(
    CreateInvitationHandler createHandler,
    AcceptInvitationHandler acceptHandler,
    ResendInvitationHandler resendHandler,
    GetInvitationByTokenHandler getByTokenHandler,
    ListPendingInvitationsHandler listHandler) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvitationRequest request,
        CancellationToken ct)
    {
        var result = await createHandler.Handle(
            new CreateInvitationCommand(request.Email, request.Role), ct);

        return result.Match<IActionResult>(
            onSuccess: invitation => CreatedAtAction(
                nameof(GetByToken),
                new { token = invitation.Id },
                invitation),
            onFailure: error => error.Code switch
            {
                "identity.user.already_exists"       => Conflict(error.ToResponse()),
                "identity.invitation.pending_exists" => Conflict(error.ToResponse()),
                _                                    => BadRequest(error.ToResponse())
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{userId:guid}/resend")]
    public async Task<IActionResult> Resend(Guid userId, CancellationToken ct)
    {
        var result = await resendHandler.Handle(new ResendInvitationCommand(userId), ct);

        return result.Match<IActionResult>(
            onSuccess: invitation => Ok(invitation),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found"         => NotFound(error.ToResponse()),
                "identity.user.already_registered" => Conflict(error.ToResponse()),
                _                                  => BadRequest(error.ToResponse())
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> ListPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeExpired = false,
        CancellationToken ct = default)
    {
        var result = await listHandler.Handle(
            new ListPendingInvitationsQuery(page, pageSize, includeExpired), ct);

        return result.Match<IActionResult>(
            onSuccess: paged => Ok(paged),
            onFailure: error => BadRequest(error.ToResponse()));
    }

    [HttpGet("validate")]
    public async Task<IActionResult> GetByToken([FromQuery] string token, CancellationToken ct)
    {
        var result = await getByTokenHandler.Handle(new GetInvitationByTokenQuery(token), ct);

        return result.Match<IActionResult>(
            onSuccess: invitation => Ok(invitation),
            onFailure: error => error.Code switch
            {
                "identity.invitation.not_found"   => NotFound(error.ToResponse()),
                "identity.invitation.expired"     => Gone(error.ToResponse()),
                "identity.invitation.already_used" => Conflict(error.ToResponse()),
                _                                  => BadRequest(error.ToResponse())
            });
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken ct)
    {
        var result = await acceptHandler.Handle(
            new AcceptInvitationCommand(
                request.Token,
                request.Forename,
                request.Surname,
                request.Password), ct);

        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => error.Code switch
            {
                "identity.invitation.not_found"    => NotFound(error.ToResponse()),
                "identity.invitation.expired"      => Gone(error.ToResponse()),
                "identity.invitation.already_used" => Conflict(error.ToResponse()),
                "identity.credentials.weak_password" => UnprocessableEntity(error.ToResponse()),
                _                                  => BadRequest(error.ToResponse())
            });
    }

    private ObjectResult Gone(object value) =>
        StatusCode(StatusCodes.Status410Gone, value);
}

public sealed record CreateInvitationRequest(string Email, UserRole Role = UserRole.User);
public sealed record AcceptInvitationRequest(
    string Token,
    string Forename,
    string Surname,
    string Password);