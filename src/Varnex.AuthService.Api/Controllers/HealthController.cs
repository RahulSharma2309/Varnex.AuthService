// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HealthController.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace Varnex.AuthService.Api.Controllers;

/// <summary>
/// Provides health check endpoints for the authentication service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the health status of the authentication service.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the health status.</returns>
    /// <response code="200">Returns the health status indicating the service is healthy.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "auth-service" });
    }
}




