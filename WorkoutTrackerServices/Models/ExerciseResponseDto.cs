namespace WorkoutTrackerServices.Models;

public class ExerciseResponseDto
{
    public int ExerciseId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public string? Description { get; set; }

    public string Category { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}