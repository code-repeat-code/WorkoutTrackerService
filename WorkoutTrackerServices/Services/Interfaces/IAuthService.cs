using WorkoutTrackerServices.Models;

namespace WorkoutTrackerServices.Services.Interfaces;

public interface IAuthService
{
    public Task<bool> SignUpAsync(SignUpRequestDto signUpRequestDto);
    public Task<TokenDto?> LoginAsync(string email, string password);
    public Task<TokenDto?> RefreshTokenAsync(TokenDto refreshTokenRequest);
    
}