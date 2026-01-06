// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthService.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Varnex.AuthService.Abstractions.DTOs;
using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Core.Repository;
using Ep.Platform.Security;
using Microsoft.Extensions.Logging;

namespace Varnex.AuthService.Core.Business;

/// <summary>
/// Implements authentication business logic operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository for data access.</param>
    /// <param name="passwordHasher">The password hasher for secure password handling.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            _logger.LogInformation("Registering new user with email: {Email}", dto.Email);

            // Hash password using Platform service
            var passwordHash = _passwordHasher.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
            };

            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully with ID: {UserId}, Email: {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user with email: {Email}", dto.Email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<(User? User, string? Error)> LoginAsync(LoginDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userRepository.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email: {Email}", dto.Email);
                return (null, "Invalid credentials");
            }

            // Verify password using Platform service
            var valid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
            if (!valid)
            {
                _logger.LogWarning("Login failed: Invalid password for email: {Email}", dto.Email);
                return (null, "Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully: {UserId}, Email: {Email}", user.Id, user.Email);
            return (user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email: {Email}", dto.Email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", dto.Email);

            var user = await _userRepository.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed: User not found for email: {Email}", dto.Email);
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password reset successful for user: {UserId}, Email: {Email}", user.Id, user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", dto.Email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", id);

            var user = await _userRepository.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
            }
            else
            {
                _logger.LogDebug("User retrieved successfully: {UserId}, Email: {Email}", user.Id, user.Email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
            throw;
        }
    }
}




