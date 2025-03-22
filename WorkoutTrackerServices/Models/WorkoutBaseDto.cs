using System.Text.Json.Serialization;

namespace WorkoutTrackerServices.Models;

public class WorkoutBaseDto
{
    [JsonIgnore] 
    public int UserId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public IList<WorkoutExerciseRequestDto> Exercises { get; set; } = new List<WorkoutExerciseRequestDto>();
}