using WorkoutTrackerServices.Entities;

namespace WorkoutTrackerServices.Repositories.Interfaces;

public interface IWorkoutRepository
{
    public Task<Workout> AddWorkoutAsync(Workout workout);
    public Task<IList<Exercise>> GetAllExerciseRefDataAsync();
    public Task<Workout> UpdateWorkoutAsync(Workout workout);
    public Task DeleteWorkoutAsync(int workoutId);
    public Task<Workout> GetWorkoutByIdAsync(int id);
    public Task<WorkoutSchedule> AddScheduleWorkoutAsync(WorkoutSchedule workoutSchedule);
    public Task<List<WorkoutSchedule>> GetUpcomingSchedulesAsync(int userId);
    public Task<List<Workout>> GetCompletedWorkoutsAsync(int userId);
    public Task UpdateWorkoutScheduleStatusAsync(int scheduleId, int userId, string newStatus);
}