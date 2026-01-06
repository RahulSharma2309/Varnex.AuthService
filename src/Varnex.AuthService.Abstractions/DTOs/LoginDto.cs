// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginDto.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Varnex.AuthService.Abstractions.DTOs;

/// <summary>
/// Data transfer object for user login.
/// </summary>
public sealed record LoginDto
{
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the user's password.
    /// </summary>
    public required string Password { get; init; }
}




