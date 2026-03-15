using Identity.Application.Invitations.Commands.ResendInvitation;

namespace Identity.Tests.Application.Invitations;

[TestFixture]
public sealed class ResendInvitationHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IInvitationRepository> _invitationRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private ResendInvitationHandler _handler = null!;

    private User _pendingUser = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _invitationRepository = new Mock<IInvitationRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _pendingUser = UserFactory.CreatePending();

        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pendingUser);

        _invitationRepository
            .Setup(r => r.GetPendingByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invitation?)null);

        _handler = new ResendInvitationHandler(
            _userRepository.Object,
            _invitationRepository.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_CreatesNewInvitation()
    {
        var result = await _handler.Handle(
            CommandFactory.ResendInvitation(userId: _pendingUser.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.IsUsed, Is.False);
        });
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CommandFactory.ResendInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }

    [Test]
    public async Task Handle_AlreadyRegisteredUser_ReturnsUserAlreadyRegistered()
    {
        var activeUser = UserFactory.CreateActive();

        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUser);

        var result = await _handler.Handle(
            CommandFactory.ResendInvitation(userId: activeUser.Id));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserAlreadyRegistered.Code));
    }

    [Test]
    public async Task Handle_ExistingPendingInvitation_InvalidatesOldOne()
    {
        var existingInvitation = InvitationFactory.CreateValid(userId: _pendingUser.Id);

        _invitationRepository
            .Setup(r => r.GetPendingByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitation);

        await _handler.Handle(
            CommandFactory.ResendInvitation(userId: _pendingUser.Id));

        Assert.That(existingInvitation.IsExpired(), Is.True);

        _invitationRepository.Verify(r =>
            r.UpdateAsync(existingInvitation, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        await _handler.Handle(
            CommandFactory.ResendInvitation(userId: _pendingUser.Id));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}