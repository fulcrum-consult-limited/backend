using Identity.Application.Users.Queries.ListUsers;

namespace Identity.Tests.Application.Users;

[TestFixture]
public sealed class ListUsersHandlerTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private ListUsersHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _handler = new ListUsersHandler(_userRepository.Object);
    }

    [Test]
    public async Task Handle_ReturnsPagedResult()
    {
        var users = new List<User>
        {
            UserFactory.CreateAdmin(email: "admin1@example.com"),
            UserFactory.CreateAdmin(email: "admin2@example.com")
        };

        _userRepository
            .Setup(r => r.ListAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<UserRole?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<User>)users, 2));

        var result = await _handler.Handle(new ListUsersQuery());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Items, Has.Count.EqualTo(2));
            Assert.That(result.Value.TotalCount, Is.EqualTo(2));
            Assert.That(result.Value.Page, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Handle_ClampsPageSizeTo100()
    {
        _userRepository
            .Setup(r => r.ListAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<UserRole?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<User>)new List<User>(), 0));

        await _handler.Handle(new ListUsersQuery(PageSize: 999));

        _userRepository.Verify(r => r.ListAsync(
            It.IsAny<int>(),
            100, // clamped
            It.IsAny<UserRole?>(), It.IsAny<bool?>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_NegativePage_DefaultsToPage1()
    {
        _userRepository
            .Setup(r => r.ListAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<UserRole?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<User>)new List<User>(), 0));

        await _handler.Handle(new ListUsersQuery(Page: -5));

        _userRepository.Verify(r => r.ListAsync(
            1,
            It.IsAny<int>(),
            It.IsAny<UserRole?>(), It.IsAny<bool?>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}