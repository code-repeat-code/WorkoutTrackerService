using System.Text.Json.Serialization;

namespace WorkoutTrackerServices.Models;

public class WorkoutUpdateRequestDto : WorkoutBaseDto
{
    [JsonIgnore]
    public int WorkoutId { get; set; }
}