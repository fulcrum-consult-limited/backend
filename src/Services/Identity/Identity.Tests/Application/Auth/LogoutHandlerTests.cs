namespace Identity.Tests.Application.Auth;

[TestFixture]
public sealed class LogoutHandlerTests
{
    private Mock<IJwtService> _jwtService = null!;
    private Mock<ITokenRevocationStore> _revocationStore = null!;
    private LogoutHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _jwtService = new Mock<IJwtService>();
        _revocationStore = new Mock<ITokenRevocationStore>();

        _handler = new LogoutHandler(
            _jwtService.Object,
            _revocationStore.Object);
    }

    [Test]
    public async Task Handle_ValidToken_RevokesToken()
    {
        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _jwtService
            .Setup(j => j.ExtractClaims(It.IsAny<string>()))
            .Returns((jti, expiresAt));

        var result = await _handler.Handle(CommandFactory.Logout());

        Assert.That(result.IsSuccess, Is.True);
        _revocationStore.Verify(r =>
            r.RevokeAsync(jti, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_AlreadyExpiredToken_SkipsRevocation()
    {
        _jwtService
            .Setup(j => j.ExtractClaims(It.IsAny<string>()))
            .Returns((Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(-1)));

        var result = await _handler.Handle(CommandFactory.Logout());

        Assert.That(result.IsSuccess, Is.True);
        _revocationStore.Verify(r =>
            r.RevokeAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ValidToken_PassesCorrectTtlToRevocationStore()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        _jwtService
            .Setup(j => j.ExtractClaims(It.IsAny<string>()))
            .Returns((Guid.NewGuid().ToString(), expiresAt));

        await _handler.Handle(CommandFactory.Logout());

        _revocationStore.Verify(r =>
            r.RevokeAsync(
                It.IsAny<string>(),
                It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero && ttl <= TimeSpan.FromMinutes(30)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}