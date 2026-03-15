using Identity.Application.PasswordReset.Commands.ChangePassword;

namespace Identity.Tests.Application.PasswordReset;

[TestFixture]
public sealed class ChangePasswordHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ChangePasswordHandler _handler = null!;

    private User _activeUser = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _activeUser = UserFactory.CreateActive();

        _userRepository
            .Setup(r => r.GetByIdAsync(_activeUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_activeUser);

        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _passwordHasher
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("newhash");

        _handler = new ChangePasswordHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ChangesPassword()
    {
        var result = await _handler.Handle(
            CommandFactory.ChangePassword(userId: _activeUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_activeUser.PasswordHash, Is.EqualTo("newhash"));
        });
    }

    [Test]
    public async Task Handle_WeakNewPassword_ReturnsWeakPassword()
    {
        var result = await _handler.Handle(
            CommandFactory.ChangePassword(userId: _activeUser.Id, newPassword: "weak"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.WeakPassword.Code));
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CommandFactory.ChangePassword());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }

    [Test]
    public async Task Handle_InactiveUser_ReturnsUserInactive()
    {
        var inactiveUser = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByIdAsync(inactiveUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        var result = await _handler.Handle(
            CommandFactory.ChangePassword(userId: inactiveUser.Id));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserInactive.Code));
    }

    [Test]
    public async Task Handle_WrongCurrentPassword_ReturnsInvalidCredentials()
    {
        _passwordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _handler.Handle(
            CommandFactory.ChangePassword(userId: _activeUser.Id));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidCredentials.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        await _handler.Handle(
            CommandFactory.ChangePassword(userId: _activeUser.Id));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}