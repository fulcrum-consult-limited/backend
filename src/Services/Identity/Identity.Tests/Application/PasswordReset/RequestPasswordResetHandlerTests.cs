using Identity.Application.PasswordReset.Commands.RequestPasswordReset;

namespace Identity.Tests.Application.PasswordReset;

[TestFixture]
public sealed class RequestPasswordResetHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordResetTokenRepository> _tokenRepository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private RequestPasswordResetHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _tokenRepository = new Mock<IPasswordResetTokenRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new RequestPasswordResetHandler(
            _userRepository.Object,
            _tokenRepository.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidEmail_CreatesResetToken()
    {
        var user = UserFactory.CreateActive(email: "user@example.com");

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            CommandFactory.RequestPasswordReset(email: "user@example.com"));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });
    }

    [Test]
    public async Task Handle_UnknownEmail_ReturnsSilentSuccess()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(CommandFactory.RequestPasswordReset());

        Assert.That(result.IsSuccess, Is.True);
        _tokenRepository.Verify(r =>
            r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_InvalidEmail_ReturnsSilentSuccess()
    {
        var result = await _handler.Handle(
            CommandFactory.RequestPasswordReset(email: "notanemail"));

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task Handle_InactiveUser_ReturnsSilentSuccess()
    {
        var user = UserFactory.CreateInactive();

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(CommandFactory.RequestPasswordReset());

        Assert.That(result.IsSuccess, Is.True);
        _tokenRepository.Verify(r =>
            r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ValidRequest_InvalidatesPreviousTokens()
    {
        var user = UserFactory.CreateActive(email: "user@example.com");

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(
            CommandFactory.RequestPasswordReset(email: "user@example.com"));

        _tokenRepository.Verify(r =>
            r.InvalidateAllForUserAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}