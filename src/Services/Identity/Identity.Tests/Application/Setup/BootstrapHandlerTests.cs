namespace Identity.Tests.Application.Setup;

[TestFixture]
public sealed class BootstrapHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private BootstrapHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _passwordHasher
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("hashedpassword");

        _handler = new BootstrapHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task Handle_WhenNoUsersExist_CreatesAdminUser()
    {
        _userRepository
            .Setup(r => r.AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = CommandFactory.Bootstrap(
            email: "admin@example.com",
            forename: "Joe",
            surname: "Bloggs");

        var result = await _handler.Handle(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Email, Is.EqualTo("admin@example.com"));
            Assert.That(result.Value.Forename, Is.EqualTo("Joe"));
            Assert.That(result.Value.Surname, Is.EqualTo("Bloggs"));
            Assert.That(result.Value.Role, Is.EqualTo("Admin"));
            Assert.That(result.Value.IsActive, Is.True);
        });
    }

    [Test]
    public async Task Handle_WhenUsersExist_ReturnsBootstrapAlreadyCompleted()
    {
        _userRepository
            .Setup(r => r.AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(CommandFactory.Bootstrap());

        Assert.That(result.Error!.Code,
            Is.EqualTo(IdentityErrors.BootstrapAlreadyCompleted.Code));
    }

    [Test]
    public async Task Handle_WithInvalidEmail_ReturnsInvalidEmail()
    {
        _userRepository
            .Setup(r => r.AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            CommandFactory.Bootstrap(email: "notanemail"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidEmail.Code));
    }

    [Test]
    public async Task Handle_WithWeakPassword_ReturnsWeakPassword()
    {
        _userRepository
            .Setup(r => r.AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            CommandFactory.Bootstrap(password: "weak"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.WeakPassword.Code));
    }

    [Test]
    public async Task Handle_Success_CommitsUnitOfWork()
    {
        _userRepository
            .Setup(r => r.AnyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(CommandFactory.Bootstrap());

        _unitOfWork.Verify(u =>
            u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}