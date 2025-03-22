namespace WorkoutTrackerServices.Models;

public class WorkoutScheduleDetailedResponseDto
{
    public int WorkoutScheduleId { get; set; }
    public int WorkoutId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public WorkoutResponseDto Workout { get; set; } = new();
}