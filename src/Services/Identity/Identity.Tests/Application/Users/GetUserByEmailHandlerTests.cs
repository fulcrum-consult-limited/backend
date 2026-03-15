using Identity.Application.Users.Queries.GetUserByEmail;

namespace Identity.Tests.Application.Users;

[TestFixture]
public sealed class GetUserByEmailHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private GetUserByEmailHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _handler = new GetUserByEmailHandler(_userRepository.Object);
    }

    [Test]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        var user = UserFactory.CreateAdmin(email: "admin@example.com");

        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            new GetUserByEmailQuery("admin@example.com"));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Email, Is.EqualTo("admin@example.com"));
        });
    }

    [Test]
    public async Task Handle_InvalidEmail_ReturnsInvalidEmail()
    {
        var result = await _handler.Handle(
            new GetUserByEmailQuery("notanemail"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.InvalidEmail.Code));
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new GetUserByEmailQuery("unknown@example.com"));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }
}