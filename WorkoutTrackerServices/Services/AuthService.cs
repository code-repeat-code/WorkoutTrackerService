using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Repositories.Interfaces;
using WorkoutTrackerServices.Services.Interfaces;

namespace WorkoutTrackerServices.Services;

public class AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    : IAuthService
{
    
    public async Task<bool> SignUpAsync(SignUpRequestDto signUpRequestDto)
    {
        try
        {
            logger.LogInformation("Attempting to register user: {Email}", signUpRequestDto.Email);
            
            if (await userRepository.GetByEmailAsync(signUpRequestDto.Email).ConfigureAwait(false) != null)
            {
                logger.LogWarning("User with username {Username} already exists", signUpRequestDto.Username);
                return false; // User already exists
            }
            
            //map reqDto to db entity
            User user = new()
            {
                Username  = signUpRequestDto.Username,
                FirstName = signUpRequestDto.FirstName,
                LastName = signUpRequestDto.LastName,
                Email = signUpRequestDto.Email,
                CreatedAt = DateTime.Now,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(signUpRequestDto.Password)
            };
            
            await userRepository.AddAsync(user).ConfigureAwait(false);

            logger.LogInformation("User {Email} registered successfully", user.Email);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while registering user: {Email}", signUpRequestDto.Email);
            throw;
        }
    }
    
    /// <summary>
    /// Logs in a user and generates an access token and refresh token.
    /// </summary>
    /// <param name="email">The email of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>The access and refresh token if login is successful, otherwise null</returns>
    public async Task<TokenDto?> LoginAsync(string email, string password)
    {
        try
        {
            logger.LogInformation("Attempting to login user: {Email}", email);

            User? user = await userRepository.GetByEmailAsync(email).ConfigureAwait(false);
            if (user == null)
            {
                logger.LogWarning("Invalid email user not found: {Email}", email);
                return null;
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                logger.LogWarning("Invalid login attempt for user: {Email}", email);
                return null;
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token and expiry time
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // Refresh token valid for 7 days
            await userRepository.UpdateUser(user);

            logger.LogInformation("User {Email} logged in successfully", email);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while logging in user: {Email}", email);
            throw;
        }
    }
    
    /// <summary>
    /// Generating the access token for user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private string GenerateAccessToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT_KEY is empty or null");
        }
       
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserId.ToString()),
                new Claim("Email", user.Email),   
                new Claim("UserName", user.Username)   
            }),
            Expires = DateTime.Now.AddMinutes(30),//token will expire in 30 minutes
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    // Generate refresh token
    private string GenerateRefreshToken()
    {
        logger.LogInformation("Generating refresh token");
        // Create a 192-byte array (256 characters when base64 encoded)
        var randomBytes = new byte[192];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes); // This will produce a 256-character string
    }
    

    /// <summary>
    /// Refreshes the access token using the refresh token.
    /// </summary>
    /// <param name="refreshTokenRequest">The refresh token</param>
    /// <returns>The new access/refresh token if refresh is successful, otherwise null</returns>
    public async Task<TokenDto?> RefreshTokenAsync(TokenDto refreshTokenRequest)
    {
        try
        {
            logger.LogInformation("Refreshing token using refresh token");

            var principal = GetPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);
            var email = principal.FindFirst("Email")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                logger.LogWarning("Unable to extract email from expired token");
                return null;
            }

            var user = await userRepository.GetByEmailAsync(email).ConfigureAwait(false);
            if (user == null || user.RefreshToken != refreshTokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                logger.LogWarning("Invalid refresh token or expired");
                return null;
            }

            string newAccessToken = GenerateAccessToken(user);
            string newRefreshToken = GenerateRefreshToken();

            // Update the refresh token and expiry time
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await userRepository.UpdateUser(user);

            logger.LogInformation("Token refreshed successfully for username: {Email}", email);
            return new TokenDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while refreshing token");
            throw;
        }
    }
    
    /// <summary>
    /// Gets princple from the expired token for refreshing the token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="SecurityTokenException"></exception>
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT_KEY is empty or null");
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false, // Allow expired tokens to be validated
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("This is Invalid Token");

        return principal;
    }
}