using System;
using System.Collections.Generic;

namespace WorkoutTrackerServices.Entities;

public partial class WorkoutReport
{
    public int WorkoutReportId { get; set; }

    public int UserId { get; set; }

    public int WorkoutId { get; set; }

    public string? ProgressNotes { get; set; }

    public DateTime? CreatedAt { get; set; }
}
