namespace WorkoutTrackerServices.Models;

public class WorkoutExerciseRequestDto
{
    public int ExerciseId { get; set; }

    public int? Sets { get; set; }

    public int? Repetitions { get; set; }

    public double? Weight { get; set; }
}