namespace Identity.Tests.Application.Invitations;

[TestFixture]
public sealed class CreateInvitationHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IInvitationRepository> _invitationRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private CreateInvitationHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _invitationRepository = new Mock<IInvitationRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _userRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _invitationRepository
            .Setup(r => r.GetPendingByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invitation?)null);

        _handler = new CreateInvitationHandler(
            _userRepository.Object,
            _invitationRepository.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_CreatesInvitation()
    {
        var command = CommandFactory.CreateInvitation();

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.IsUsed, Is.False);
        });
    }

    [Test]
    public async Task Handle_InvalidEmail_ReturnsInvalidEmail()
    {
        var result = await _handler.Handle(
            CommandFactory.CreateInvitation(email: "notanemail"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidEmail.Code));
    }

    [Test]
    public async Task Handle_DuplicateEmail_ReturnsUserAlreadyExists()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(CommandFactory.CreateInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserAlreadyExists.Code));
    }

    [Test]
    public async Task Handle_PendingInvitationExists_ReturnsPendingInvitationExists()
    {
        _invitationRepository
            .Setup(r => r.GetPendingByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InvitationFactory.CreateValid());

        var result = await _handler.Handle(CommandFactory.CreateInvitation());

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.PendingInvitationExists.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        await _handler.Handle(CommandFactory.CreateInvitation());

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Success_AddsUserAndInvitation()
    {
        await _handler.Handle(CommandFactory.CreateInvitation());

        _userRepository.Verify(r =>
            r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);

        _invitationRepository.Verify(r =>
            r.AddAsync(It.IsAny<Invitation>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}