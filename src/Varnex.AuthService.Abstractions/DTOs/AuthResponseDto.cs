// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthResponseDto.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Varnex.AuthService.Abstractions.DTOs;

/// <summary>
/// Data transfer object for authentication response containing token and user information.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Gets or sets the JWT authentication token.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's unique identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's email address.
    /// </summary>
    public string Email { get; set; } = null!;
}




