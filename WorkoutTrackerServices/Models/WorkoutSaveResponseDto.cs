namespace WorkoutTrackerServices.Models;

public class WorkoutSaveResponseDto
{
    public int WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty; 
}