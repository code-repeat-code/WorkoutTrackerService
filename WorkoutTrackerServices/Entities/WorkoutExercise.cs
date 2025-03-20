using System;
using System.Collections.Generic;

namespace WorkoutTrackerServices.Entities;

public partial class WorkoutExercise
{
    public int WorkoutExerciseId { get; set; }

    public int WorkoutId { get; set; }

    public int ExerciseId { get; set; }

    public int? Sets { get; set; }

    public int? Repetitions { get; set; }

    public double? Weight { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual Workout Workout { get; set; } = null!;
}
