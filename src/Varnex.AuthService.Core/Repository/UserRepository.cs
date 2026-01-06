// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Varnex.AuthService.Core.Repository;

/// <summary>
/// Implements user data access operations using Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserRepository(AppDbContext db, ILogger<UserRepository> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task AddAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug("Adding new user to database: {Email}", user.Email);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User added successfully: {UserId}, Email: {Email}", user.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user to database: {Email}", user.Email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<User?> FindByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
        }

        try
        {
            _logger.LogDebug("Searching for user by email: {Email}", email);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogDebug("User not found with email: {Email}", email);
            }
            else
            {
                _logger.LogDebug("User found: {UserId}, Email: {Email}", user.Id, user.Email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for user by email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<User?> FindByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Searching for user by ID: {UserId}", id);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogDebug("User not found with ID: {UserId}", id);
            }
            else
            {
                _logger.LogDebug("User found: {UserId}, Email: {Email}", user.Id, user.Email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for user by ID: {UserId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
        }

        try
        {
            _logger.LogDebug("Checking if email exists: {Email}", email);
            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            _logger.LogDebug("Email exists check result for {Email}: {Exists}", email, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug("Updating user in database: {UserId}, Email: {Email}", user.Id, user.Email);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User updated successfully: {UserId}, Email: {Email}", user.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user: {UserId}, Email: {Email}", user.Id, user.Email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            await _db.SaveChangesAsync();
            _logger.LogDebug("Changes saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes to database");
            throw;
        }
    }
}




