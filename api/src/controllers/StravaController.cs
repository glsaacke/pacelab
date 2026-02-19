using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.src.services;

namespace api.src.controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaController : ControllerBase
{
    private readonly IStravaService _stravaService;
    private readonly ILogger<StravaController> _logger;

    public StravaController(IStravaService stravaService, ILogger<StravaController> logger)
    {
        _stravaService = stravaService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the Strava OAuth authorization URL to redirect the user to.
    /// The frontend should redirect the user's browser to the returned URL.
    /// </summary>
    [Authorize]
    [HttpGet("authorize")]
    public ActionResult<object> Authorize()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var authorizationUrl = _stravaService.GetAuthorizationUrl(userId);

            return Ok(new { authorizationUrl });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Strava configuration error in authorize endpoint");
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building Strava authorization URL");
            return StatusCode(500, new { error = "An error occurred while building the Strava authorization URL" });
        }
    }

    /// <summary>
    /// Handles the OAuth callback from Strava. Exchanges the authorization code for tokens
    /// and links the Strava account to the user identified by the state parameter.
    /// </summary>
    /// <param name="code">Authorization code provided by Strava.</param>
    /// <param name="state">Encoded user ID passed through the OAuth state parameter.</param>
    /// <param name="error">Set by Strava if the user denied access.</param>
    [HttpGet("callback")]
    public async Task<ActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        // TODO: After saving tokens, redirect the user to the frontend dashboard.
        //       Update the redirect URL below once the frontend URL is known.
        //       e.g. return Redirect($"{frontendUrl}/dashboard?strava=connected");

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Strava OAuth denied by user. Error: {Error}", error);
            return BadRequest(new { error = "Strava authorization was denied", details = error });
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest(new { error = "Missing authorization code from Strava" });
        }

        if (!int.TryParse(state, out int userId))
        {
            return BadRequest(new { error = "Invalid state parameter" });
        }

        try
        {
            await _stravaService.HandleCallbackAsync(code, userId);
            return Ok(new { message = "Strava account connected successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error handling Strava callback for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling Strava callback for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while connecting your Strava account" });
        }
    }
}
