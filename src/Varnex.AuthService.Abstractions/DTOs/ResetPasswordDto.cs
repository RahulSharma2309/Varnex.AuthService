// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResetPasswordDto.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Varnex.AuthService.Abstractions.DTOs;

/// <summary>
/// Data transfer object for resetting a user's password.
/// </summary>
public sealed record ResetPasswordDto
{
    /// <summary>
    /// Gets the email address of the user whose password is to be reset.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the new password for the user.
    /// </summary>
    public required string NewPassword { get; init; }
}




