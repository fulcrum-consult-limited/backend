namespace Identity.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController(
    GetUserByIdHandler getByIdHandler,
    GetUserByEmailHandler getByEmailHandler,
    ListUsersHandler listHandler,
    DeactivateUserHandler deactivateHandler,
    ReactivateUserHandler reactivateHandler,
    UpdateUserRoleHandler updateRoleHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await listHandler.Handle(
            new ListUsersQuery(page, pageSize, role, isActive, search), ct);

        return result.Match<IActionResult>(
            onSuccess: paged => Ok(paged),
            onFailure: error => BadRequest(error.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await getByIdHandler.Handle(new GetUserByIdQuery(id), ct);

        return result.Match<IActionResult>(
            onSuccess: user => Ok(user),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found" => NotFound(error.ToResponse()),
                _                         => BadRequest(error.ToResponse())
            });
    }

    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        var result = await getByEmailHandler.Handle(new GetUserByEmailQuery(email), ct);

        return result.Match<IActionResult>(
            onSuccess: user => Ok(user),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found"            => NotFound(error.ToResponse()),
                _                                    => BadRequest(error.ToResponse())
            });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await deactivateHandler.Handle(new DeactivateUserCommand(id), ct);

        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found" => NotFound(error.ToResponse()),
                _                         => BadRequest(error.ToResponse())
            });
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        var result = await reactivateHandler.Handle(new ReactivateUserCommand(id), ct);

        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found" => NotFound(error.ToResponse()),
                _                         => BadRequest(error.ToResponse())
            });
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        var result = await updateRoleHandler.Handle(
            new UpdateUserRoleCommand(id, request.Role), ct);

        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => error.Code switch
            {
                "identity.user.not_found" => NotFound(error.ToResponse()),
                "identity.user.inactive"  => Conflict(error.ToResponse()),
                _                         => BadRequest(error.ToResponse())
            });
    }
}

public sealed record UpdateRoleRequest(UserRole Role);