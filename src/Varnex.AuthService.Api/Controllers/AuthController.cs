// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthController.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Security.Claims;
using System.Text.RegularExpressions;
using Varnex.AuthService.Abstractions.DTOs;
using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Core.Business;
using Varnex.AuthService.Core.Data;
using Ep.Platform.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Varnex.AuthService.Api.Controllers;

/// <summary>
/// Provides authentication endpoints for user registration, login, password reset, and profile retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <param name="jwtTokenGenerator">The JWT token generator.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpClientFactory">The HTTP client factory for external service calls.</param>
    /// <param name="db">The database context.</param>
    public AuthController(
        IAuthService authService,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthController> logger,
        IHttpClientFactory httpClientFactory,
        AppDbContext db)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">The registration data transfer object.</param>
    /// <returns>An <see cref="IActionResult"/> containing the created user information or error details.</returns>
    /// <response code="201">Returns the newly created user.</response>
    /// <response code="400">If the registration data is invalid.</response>
    /// <response code="409">If the email or phone number already exists.</response>
    /// <response code="500">If an internal server error occurs.</response>
    /// <response code="503">If the user service is unavailable.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var validationError = ValidateRegisterDto(dto);
        if (validationError is not null)
        {
            return validationError;
        }

        try
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                return Conflict(new { error = "Email already registered" });
            }

            var phoneValidation = await EnsurePhoneUniqueAsync(dto.PhoneNumber);
            if (phoneValidation is not null)
            {
                return phoneValidation;
            }

            var user = await _authService.RegisterAsync(dto);

            var profileResult = await CreateProfileAsync(dto, user);
            if (profileResult is not null)
            {
                return profileResult;
            }

            return CreatedAtAction(nameof(Me), new { id = user.Id }, new { user.Id, user.Email, user.FullName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
            return StatusCode(500, new { error = "Registration failed. Please try again later." });
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="dto">The login data transfer object containing email and password.</param>
    /// <returns>An <see cref="IActionResult"/> containing the authentication token or error details.</returns>
    /// <response code="200">Returns the authentication token and user information.</response>
    /// <response code="400">If the login data is invalid.</response>
    /// <response code="401">If the credentials are invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { error = "Email and password required" });
        }

        var (user, error) = await _authService.LoginAsync(dto);
        if (user == null || error != null)
        {
            return Unauthorized(new { error });
        }

        // Use Platform's JWT token generator
        var claims = new Dictionary<string, string>
        {
            [ClaimTypes.NameIdentifier] = user.Id.ToString(),
            [ClaimTypes.Email] = user.Email,
            ["fullName"] = user.FullName ?? string.Empty,
        };
        var token = _jwtTokenGenerator.GenerateToken(claims);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresIn = 6 * 60 * 60,
            UserId = user.Id,
            Email = user.Email,
        });
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="dto">The reset password data transfer object containing email and new password.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the password reset operation.</returns>
    /// <response code="200">If the password was reset successfully.</response>
    /// <response code="400">If the reset data is invalid.</response>
    /// <response code="404">If the user was not found.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { error = "Email and new password required" });
        }

        var success = await _authService.ResetPasswordAsync(dto);
        if (!success)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(new { status = "password reset" });
    }

    /// <summary>
    /// Retrieves the authenticated user's profile information.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the user's profile information.</returns>
    /// <response code="200">Returns the user's profile information.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the user was not found.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(id, out var guid))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(guid);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new { user.Id, user.Email, user.FullName, user.CreatedAt });
    }

    /// <summary>
    /// Response model for phone number existence check.
    /// </summary>
    private sealed class PhoneExistsResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the phone number exists.
        /// </summary>
        public bool Exists { get; set; } = false;
    }

    private IActionResult? ValidateRegisterDto(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { error = "Full name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return BadRequest(new { error = "Invalid email format" });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { error = "Password is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            return BadRequest(new { error = "Confirm password is required" });
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest(new { error = "Passwords do not match" });
        }

        if (!Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"))
        {
            return BadRequest(new { error = "Password must be 8+ chars, include upper, lower, number, special" });
        }

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            return BadRequest(new { error = "Phone number is required" });
        }

        if (!Regex.IsMatch(dto.PhoneNumber, @"^\+?\d{10,15}$"))
        {
            return BadRequest(new { error = "Invalid phone number format (10-15 digits, optional + prefix)" });
        }

        return null;
    }

    private async Task<IActionResult?> EnsurePhoneUniqueAsync(string phoneNumber)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("user");
            var phoneCheckResponse = await httpClient.GetAsync($"/api/users/phone-exists/{Uri.EscapeDataString(phoneNumber)}");
            if (!phoneCheckResponse.IsSuccessStatusCode)
            {
                _logger.LogError("User Service returned non-success status {StatusCode} when checking phone number", phoneCheckResponse.StatusCode);
                return StatusCode(503, new { error = "Unable to validate phone number. Please try again later." });
            }

            var phoneCheckResult = await phoneCheckResponse.Content.ReadFromJsonAsync<PhoneExistsResponse>();
            if (phoneCheckResult?.Exists == true)
            {
                return Conflict(new { error = "Phone number already registered" });
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check phone number existence in User Service. Registration aborted.");
            return StatusCode(503, new { error = "Unable to validate phone number. Please try again later." });
        }
    }

    private async Task<IActionResult?> CreateProfileAsync(RegisterDto dto, User user)
    {
        try
        {
            var names = dto.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = names.Length > 0 ? names[0] : string.Empty;
            var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : string.Empty;

            var httpClient = _httpClientFactory.CreateClient("user");
            var profileDto = new
            {
                UserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
            };

            var profileResponse = await httpClient.PostAsJsonAsync("/api/users", profileDto);

            if (profileResponse.IsSuccessStatusCode)
            {
                return null;
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            var errorContent = await profileResponse.Content.ReadAsStringAsync();
            _logger.LogError("Profile creation failed with status {StatusCode}: {Error}. Auth user rolled back.", profileResponse.StatusCode, errorContent);

            if (profileResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                try
                {
                    var errorObj = await profileResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    if (errorObj.TryGetProperty("error", out var errorProp))
                    {
                        return Conflict(new { error = errorProp.GetString() });
                    }
                }
                catch
                {
                    // Ignore parsing errors and fall back to a default message.
                }

                return Conflict(new { error = "Phone number already registered" });
            }

            return StatusCode((int)profileResponse.StatusCode, new { error = "Failed to create user profile. Registration aborted." });
        }
        catch (Exception ex)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            _logger.LogError(ex, "Profile creation failed. Auth user rolled back for {Email}", dto.Email);
            return StatusCode(500, new { error = "Failed to create user profile. Registration aborted." });
        }
    }
}




