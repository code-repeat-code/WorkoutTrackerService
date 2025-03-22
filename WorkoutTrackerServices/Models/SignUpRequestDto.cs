using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerServices.Models;

public class SignUpRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    public string? LastName { get; set; }
    
    [Required]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}