namespace WorkoutTrackerServices.Models;

public class WorkoutScheduleResponseDto
{
    public int WorkoutScheduleId { get; set; }
    public int WorkoutId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; }  = string.Empty;
    public IList<WorkoutExerciseResponseDto> Exercises { get; set; } = new List<WorkoutExerciseResponseDto>();
}