using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Models.ReportModels;

namespace WorkoutTrackerServices.Services.Interfaces;

public interface IWorkoutService
{
    public Task<Workout> AddWorkoutAsync(WorkoutRequestDto workout);
    public Task<IList<Exercise>> GetAllExerciseRefDataAsync();
    public Task<Workout> UpdateWorkoutAsync(WorkoutUpdateRequestDto workout);
    public Task DeleteWorkoutAsync(int workoutId,int userId);
    public Task<WorkoutScheduleResponseDto> AddWorkoutScheduleAsync(WorkoutScheduleRequestDto workoutSchedule, int userId);
    public Task<List<WorkoutScheduleDetailedResponseDto>> GetUpcomingSchedulesAsync(int userId);
    public Task<List<WorkoutReportResponseDto>> GenerateWorkoutReportsAsync(int userId);
    public Task UpdateWorkoutScheduleStatusAsync(int scheduleId, int userId, string newStatus);
}