using Identity.Application.Users.Commands.UpdateUserRole;

namespace Identity.Tests.Application.Users;

[TestFixture]
public sealed class UpdateUserRoleHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private UpdateUserRoleHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new UpdateUserRoleHandler(
            _userRepository.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_UpdatesRole()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            CommandFactory.UpdateUserRole(userId: user.Id, role: UserRole.User));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.Role, Is.EqualTo(UserRole.User));
        });
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CommandFactory.UpdateUserRole());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }

    [Test]
    public async Task Handle_InactiveUser_ReturnsUserInactive()
    {
        var user = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            CommandFactory.UpdateUserRole(userId: user.Id));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserInactive.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(
            CommandFactory.UpdateUserRole(userId: user.Id, role: UserRole.User));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}