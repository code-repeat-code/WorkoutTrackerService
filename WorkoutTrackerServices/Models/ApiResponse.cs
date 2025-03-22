using System.Text.Json.Serialization;

namespace WorkoutTrackerServices.Models;

public class ApiResponse<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Page { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PageSize { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Pages { get; set; }
    public IList<T>? Data { get; set; }
}