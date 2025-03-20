namespace WorkoutTrackerServices.Models;

public class ApiResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Pages { get; set; }
    public IList<T>? Data { get; set; }
}