namespace WorkoutTrackerServices.Models.ReportModels;

public class WorkoutExerciseReportDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Repetitions { get; set; }
    public float Weight { get; set; }
    public string Category { get; set; } = string.Empty;
}
