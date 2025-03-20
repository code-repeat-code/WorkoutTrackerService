using System;
using System.Collections.Generic;

namespace WorkoutTrackerServices.Entities;

public partial class Workout
{
    public int WorkoutId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();

    public virtual ICollection<WorkoutReport> WorkoutReports { get; set; } = new List<WorkoutReport>();

    public virtual ICollection<WorkoutSchedule> WorkoutSchedules { get; set; } = new List<WorkoutSchedule>();
}
