namespace WorkoutTrackerServices.Models.ReportModels;

public class WorkoutReportResponseDto
{
    public int WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<WorkoutExerciseReportDto> Exercises { get; set; } = new();
}
