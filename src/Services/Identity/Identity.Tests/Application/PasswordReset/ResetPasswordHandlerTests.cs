using Identity.Application.PasswordReset.Commands.ResetPassword;

namespace Identity.Tests.Application.PasswordReset;

[TestFixture]
public sealed class ResetPasswordHandlerTests
{
    private Mock<IPasswordResetTokenRepository> _tokenRepository = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ResetPasswordHandler _handler = null!;

    private User _activeUser = null!;
    private PasswordResetToken _token = null!;

    [SetUp]
    public void SetUp()
    {
        _tokenRepository = new Mock<IPasswordResetTokenRepository>();
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _activeUser = UserFactory.CreateActive();
        _token = PasswordResetTokenFactory.CreateValid(userId: _activeUser.Id);

        _tokenRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_token);

        _userRepository
            .Setup(r => r.GetByIdAsync(_activeUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_activeUser);

        _passwordHasher
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("newhash");

        _handler = new ResetPasswordHandler(
            _tokenRepository.Object,
            _userRepository.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidToken_ResetsPassword()
    {
        var result = await _handler.Handle(
            CommandFactory.ResetPassword(token: _token.Token));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_token.IsUsed, Is.True);
            Assert.That(_activeUser.PasswordHash, Is.EqualTo("newhash"));
        });
    }

    [Test]
    public async Task Handle_WeakPassword_ReturnsWeakPassword()
    {
        var result = await _handler.Handle(
            CommandFactory.ResetPassword(token: _token.Token, newPassword: "weak"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.WeakPassword.Code));
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsTokenNotFound()
    {
        _tokenRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var result = await _handler.Handle(CommandFactory.ResetPassword());

        Assert.That(result.Error!.Code,
            Is.EqualTo(IdentityErrors.PasswordResetTokenNotFound.Code));
    }

    [Test]
    public async Task Handle_ExpiredToken_ReturnsTokenExpired()
    {
        var expiredToken = PasswordResetTokenFactory.CreateExpired(userId: _activeUser.Id);

        _tokenRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var result = await _handler.Handle(
            CommandFactory.ResetPassword(token: expiredToken.Token));

        Assert.That(result.Error!.Code,
            Is.EqualTo(IdentityErrors.PasswordResetTokenExpired.Code));
    }

    [Test]
    public async Task Handle_UsedToken_ReturnsTokenAlreadyUsed()
    {
        var usedToken = PasswordResetTokenFactory.CreateUsed(userId: _activeUser.Id);

        _tokenRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usedToken);

        var result = await _handler.Handle(
            CommandFactory.ResetPassword(token: usedToken.Token));

        Assert.That(result.Error!.Code,
            Is.EqualTo(IdentityErrors.PasswordResetTokenAlreadyUsed.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        await _handler.Handle(CommandFactory.ResetPassword(token: _token.Token));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}