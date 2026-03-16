using System.Collections.Concurrent;
using CamE0.Authentication.Interfaces;
using CamE0.Authentication.Models;

namespace CamE0.Authentication.Services;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public InMemoryUserRepository()
    {
        // Seed default admin user (password: admin)
        var adminUser = new User
        {
            Id = "1",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", workFactor: 12),
            Role = UserRoles.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.TryAdd(adminUser.Id, adminUser);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(string id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync()
    {
        IReadOnlyList<User> users = _users.Values.ToList().AsReadOnly();
        return Task.FromResult(users);
    }

    public Task<User> CreateAsync(User user)
    {
        if (!_users.TryAdd(user.Id, user))
        {
            throw new InvalidOperationException($"User with ID {user.Id} already exists.");
        }
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task DeleteAsync(string id)
    {
        _users.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string username)
    {
        var exists = _users.Values.Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}
