using System.Text.Json.Serialization;

namespace WorkoutTrackerServices.Models;
/// <summary>
/// Request DTO to schedule a workout on a specific date and time.
/// </summary>
/// <example>
/// {
///   "workoutId": 12,
///   "scheduledDate": "2025-03-23T06:30:00"
/// }
/// </example>
public class WorkoutScheduleRequestDto
{
    /// <summary>
    /// The ID of the workout to be scheduled.
    /// </summary>
    ///<example>1</example>
    public int WorkoutId { get; set; }
    /// <summary>
    /// The date and time the workout is scheduled for (local time).
    /// </summary>
    ///<example>2025-03-25T06:30:00</example>
    public DateTime ScheduledDate { get; set; }
}