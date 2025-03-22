namespace WorkoutTrackerServices.Models;

public class WorkoutResponseDto
{
    public int WorkoutId { get; set; }
    public int UserId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<WorkoutExerciseScheduleResponseDto> WorkoutExercises { get; set; } = new();    
}