namespace Identity.Tests.Application.Invitations;

[TestFixture]
public sealed class AcceptInvitationHandlerTests
{
    private Mock<IInvitationRepository> _invitationRepository = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private AcceptInvitationHandler _handler = null!;

    private User _pendingUser = null!;
    private Invitation _invitation = null!;

    [SetUp]
    public void SetUp()
    {
        _invitationRepository = new Mock<IInvitationRepository>();
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _pendingUser = UserFactory.CreatePending();
        _invitation = InvitationFactory.CreateValid(userId: _pendingUser.Id);

        _invitationRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_invitation);

        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pendingUser);

        _passwordHasher
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("hashedpassword");

        _handler = new AcceptInvitationHandler(
            _invitationRepository.Object,
            _userRepository.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ActivatesUser()
    {
        var command = CommandFactory.AcceptInvitation(token: _invitation.Token);

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_pendingUser.IsActive, Is.True);
            Assert.That(_pendingUser.Forename, Is.EqualTo(command.Forename));
            Assert.That(_pendingUser.Surname, Is.EqualTo(command.Surname));
        });
    }

    [Test]
    public async Task Handle_WeakPassword_ReturnsWeakPassword()
    {
        var result = await _handler.Handle(
            CommandFactory.AcceptInvitation(
                token: _invitation.Token,
                password: "weak"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.WeakPassword.Code));
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsInvitationNotFound()
    {
        _invitationRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invitation?)null);

        var result = await _handler.Handle(CommandFactory.AcceptInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvitationNotFound.Code));
    }

    [Test]
    public async Task Handle_ExpiredInvitation_ReturnsInvitationExpired()
    {
        _invitationRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InvitationFactory.CreateExpired(userId: _pendingUser.Id));

        var result = await _handler.Handle(CommandFactory.AcceptInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvitationExpired.Code));
    }

    [Test]
    public async Task Handle_AlreadyUsedInvitation_ReturnsInvitationAlreadyUsed()
    {
        _invitationRepository
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InvitationFactory.CreateUsed(userId: _pendingUser.Id));

        var result = await _handler.Handle(CommandFactory.AcceptInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvitationAlreadyUsed.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        await _handler.Handle(
            CommandFactory.AcceptInvitation(token: _invitation.Token));

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}