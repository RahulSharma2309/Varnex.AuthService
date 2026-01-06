// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthService.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Varnex.AuthService.Abstractions.DTOs;
using Varnex.AuthService.Abstractions.Models;

namespace Varnex.AuthService.Core.Business;

/// <summary>
/// Defines the contract for authentication business logic operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="dto">The registration data transfer object containing user information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created user.</returns>
    Task<User> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticates a user with their credentials.
    /// </summary>
    /// <param name="dto">The login data transfer object containing email and password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authenticated user or an error message.</returns>
    Task<(User? User, string? Error)> LoginAsync(LoginDto dto);

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="dto">The reset password data transfer object containing email and new password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation succeeded.</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    Task<User?> GetUserByIdAsync(Guid id);
}




