using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;
using CamE0.Authentication.Services;

namespace CamE0.Tests.Authentication;

public class UserRepositoryTests
{
    private readonly IUserRepository _repository = new InMemoryUserRepository();

    [Fact]
    public async Task GetByUsernameAsync_DefaultAdmin_ReturnsAdmin()
    {
        var user = await _repository.GetByUsernameAsync("admin");
        Assert.NotNull(user);
        Assert.Equal(UserRoles.Admin, user.Role);
    }

    [Fact]
    public async Task CreateAsync_NewUser_CanBeRetrieved()
    {
        var newUser = new User
        {
            Username = "testuser",
            PasswordHash = "hash",
            Role = UserRoles.Viewer
        };

        await _repository.CreateAsync(newUser);
        var retrieved = await _repository.GetByUsernameAsync("testuser");

        Assert.NotNull(retrieved);
        Assert.Equal("testuser", retrieved.Username);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var user = new User { Id = "delete-me", Username = "todelete", PasswordHash = "hash" };
        await _repository.CreateAsync(user);
        await _repository.DeleteAsync("delete-me");

        var result = await _repository.GetByIdAsync("delete-me");
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingUser_ReturnsTrue()
    {
        Assert.True(await _repository.ExistsAsync("admin"));
    }

    [Fact]
    public async Task ExistsAsync_NonExistentUser_ReturnsFalse()
    {
        Assert.False(await _repository.ExistsAsync("nobody"));
    }

    [Fact]
    public async Task GetAllAsync_IncludesDefaultAdmin()
    {
        var users = await _repository.GetAllAsync();
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Username == "admin");
    }
}
