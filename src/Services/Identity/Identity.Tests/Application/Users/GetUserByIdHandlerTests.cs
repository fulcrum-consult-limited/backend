namespace Identity.Tests.Application.Users;

[TestFixture]
public sealed class GetUserByIdHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private GetUserByIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _handler = new GetUserByIdHandler(_userRepository.Object);
    }

    [Test]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        var user = UserFactory.CreateAdmin();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new GetUserByIdQuery(user.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(user.Id));
            Assert.That(result.Value.Email, Is.EqualTo(user.Email.Value));
            Assert.That(result.Value.Forename, Is.EqualTo(user.Forename));
            Assert.That(result.Value.Surname, Is.EqualTo(user.Surname));
            Assert.That(result.Value.Role, Is.EqualTo("Admin"));
            Assert.That(result.Value.IsActive, Is.True);
        });
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(new GetUserByIdQuery(Guid.NewGuid()));

        Assert.That(result.Error!.Code, Is.EqualTo(IdentityErrors.UserNotFound.Code));
    }
}