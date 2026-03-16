namespace Identity.Tests.Application.Auth;

[TestFixture]
public sealed class LoginHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IJwtService> _jwtService = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private LoginHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _jwtService = new Mock<IJwtService>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new LoginHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtService.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtService
            .Setup(j => j.Generate(user))
            .Returns(("token123", "jti123", DateTime.UtcNow.AddHours(1)));

        var result = await _handler.Handle(
            CommandFactory.Login(email: user.Email.Value));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.AccessToken, Is.EqualTo("token123"));
            Assert.That(result.Value.TokenType, Is.EqualTo("Bearer"));
        });
    }

    [Test]
    public async Task Handle_InvalidEmail_ReturnsInvalidCredentials()
    {
        var result = await _handler.Handle(
            CommandFactory.Login(email: "notanemail"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidCredentials.Code));
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsInvalidCredentials()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _handler.Handle(CommandFactory.Login());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidCredentials.Code));
    }

    [Test]
    public async Task Handle_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _handler.Handle(
            CommandFactory.Login(email: user.Email.Value));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidCredentials.Code));
    }

    [Test]
    public async Task Handle_InactiveUser_ReturnsUserInactive()
    {
        var user = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var result = await _handler.Handle(
            CommandFactory.Login(email: user.Email.Value));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserInactive.Code));
    }

    [Test]
    public async Task Handle_ValidLogin_RecordsLoginAndCommits()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtService
            .Setup(j => j.Generate(user))
            .Returns(("token", "jti", DateTime.UtcNow.AddHours(1)));

        await _handler.Handle(CommandFactory.Login(email: user.Email.Value));

        Assert.That(user.LastLoginAt, Is.Not.Null);
        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}