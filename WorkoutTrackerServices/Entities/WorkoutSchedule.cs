using System;
using System.Collections.Generic;

namespace WorkoutTrackerServices.Entities;

public partial class WorkoutSchedule
{
    public int WorkoutScheduleId { get; set; }

    public int WorkoutId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Workout Workout { get; set; } = null!;
}
