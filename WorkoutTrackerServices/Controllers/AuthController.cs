using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Repositories.Interfaces;
using WorkoutTrackerServices.Services.Interfaces;

namespace WorkoutTrackerServices.Controllers;

[ApiController]
[Route("/v1/authentication")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger, IUserRepository userRepository)
    : ControllerBase
{
    /// <summary>
    /// User registration or sign-up.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Signup([FromBody] SignUpRequestDto request)
    {
        try
        {
            // Validate the username and password
            ValidateUsername(request.Username); 
            ValidatePassword(request.Password);

            // Proceed with registration logic
            bool result = await authService.SignUpAsync(request).ConfigureAwait(false);

            if (!result)
            {
                // Username already exists
                return BadRequest(new ApiResponse<string> {Data = new List<string> { "User already exists." } });
            }

            //Return success response
            return Ok(new ApiResponse<string> { Data = new List<string> { "Success to Sign Up" }});
        }
        catch (ArgumentException ex)
        {
            // Handle validation errors by returning 400 with the error message
            logger.LogWarning(ex, "Validation failed during signup.");
            return StatusCode(400,new ApiResponse<string> { Data = new List<string> { "Validation failed during signup" }});
        }
        catch (Exception ex)
        {
            // Catch any unexpected errors and return a 500 internal server error
            logger.LogError(ex, "Error during signup.");
            return StatusCode(500, new ApiResponse<string> { Data = new List<string> { "An error occurred. Please try again." }});
        }
    }
    
    /// <summary>
    /// Endpoint for user login.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        ApiResponse<TokenDto> result = new();
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for Login request: {Request}", request);
            return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string> { Data = new List<string> { "Invalid model state." }});
        }

        try
        {
            logger.LogInformation("Login attempt for username: {Email}", request.Email);

            TokenDto? tokenResponse = await authService.LoginAsync(request.Email, request.Password);
            if (tokenResponse == null)
            {
                logger.LogWarning("Invalid login attempt for username: {Email}", request.Email);
                return StatusCode(StatusCodes.Status401Unauthorized,new ApiResponse<string> { Data = new List<string> { "Unauthorized access attempt.." }});
            }

            logger.LogInformation("User logged in successfully: {Email}", request.Email);
            result.Data = new List<TokenDto>{tokenResponse};
            return StatusCode(StatusCodes.Status200OK, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Login for username: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Data = new List<string> { "An error occurred. Please try again." }});
        }
    }
    
    /// <summary>
    /// Endpoint to refresh the user's access token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("tokenRefresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> RefreshToken([FromBody] TokenDto request)
    {
        try
        {
            logger.LogInformation("Token refresh attempt for refresh token: {RefreshToken}", request.RefreshToken);

            TokenDto? tokenResponse = await authService.RefreshTokenAsync(request);
            if (tokenResponse == null)
            {
                logger.LogWarning("Invalid or expired refresh token: {RefreshToken}", request.RefreshToken);
                return StatusCode(StatusCodes.Status401Unauthorized ,new ApiResponse<string> { Data = new List<string>{"Invalid or expired refresh token."}});
            }

            logger.LogInformation("Token refreshed successfully.");
            return Ok(new ApiResponse<object> { Data = [tokenResponse] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh for refresh token: {RefreshToken}", request.RefreshToken);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> {Data = new List<string>{"An error occurred while refreshing the token. Please try again." }});
        }
    }
    
    private void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty.");
        }

        if (username.Length < 3 || username.Length > 20)
        {
            throw new ArgumentException("Username must be between 3 and 20 characters.");
        }

        if (!username.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Username can only contain alphanumeric characters.");
        }
    }
    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        if (password.Length < 8 || password.Length > 25)
        {
            throw new ArgumentException("Password must be between 8 and 25 characters.");
        }

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            throw new ArgumentException("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
        }
    }
}