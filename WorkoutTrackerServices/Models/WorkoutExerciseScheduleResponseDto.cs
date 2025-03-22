namespace WorkoutTrackerServices.Models;

public class WorkoutExerciseScheduleResponseDto : WorkoutExerciseResponseDto
{
    public ExerciseResponseDto Exercise { get; set; } = new();
}