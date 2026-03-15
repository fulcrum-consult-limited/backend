using Identity.Application.Users.Commands.ReactivateUser;

namespace Identity.Tests.Application.Users;

[TestFixture]
public sealed class ReactivateUserHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ReactivateUserHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new ReactivateUserHandler(
            _userRepository.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_InactiveUser_ReactivatesUser()
    {
        var user = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            CommandFactory.ReactivateUser(userId: user.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.IsActive, Is.True);
        });
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CommandFactory.ReactivateUser());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }

    [Test]
    public async Task Handle_AlreadyActiveUser_ReturnsSuccessIdempotently()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            CommandFactory.ReactivateUser(userId: user.Id));

        Assert.That(result.IsSuccess, Is.True);
        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        var user = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(CommandFactory.ReactivateUser(userId: user.Id));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}