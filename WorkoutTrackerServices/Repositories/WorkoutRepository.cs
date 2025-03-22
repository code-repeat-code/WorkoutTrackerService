using Microsoft.EntityFrameworkCore;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Repositories.Interfaces;

namespace WorkoutTrackerServices.Repositories;

public class WorkoutRepository(WorkoutContext workoutContext, ILogger<WorkoutRepository> logger) : IWorkoutRepository
{
    public async Task<Workout> AddWorkoutAsync(Workout workout)
    {
        try
        {
            await workoutContext.Workouts.AddAsync(workout).ConfigureAwait(false);
            await workoutContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"Error occured while adding workout.");
            throw;
        }
        return workout;
    }
    
    /// <summary>
    /// Gets exercise ref data.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IList<Exercise>> GetAllExerciseRefDataAsync()
    {
        return await workoutContext.Exercises.ToListAsync().ConfigureAwait(false);
    }
    
    /// <summary>
    /// Updates an existing workout and replaces its exercises.
    /// </summary>
    /// <param name="workout">Workout entity with updated values</param>
    /// <returns>The updated Workout</returns>
    public async Task<Workout> UpdateWorkoutAsync(Workout workout)
    {
        // Load existing workout including its exercises
        Workout? existingWorkout = await workoutContext.Workouts
            .Include(w => w.WorkoutExercises)
            .FirstOrDefaultAsync(w => w.WorkoutId == workout.WorkoutId && w.IsDeleted == false).ConfigureAwait(false);

        if (existingWorkout is null)
        {
            throw new KeyNotFoundException($"Workout with ID {workout.WorkoutId} not found or deleted.");
        }

        // Update core fields
        existingWorkout.WorkoutName = workout.WorkoutName;
        existingWorkout.Comment = workout.Comment;
        existingWorkout.LastUpdatedAt = workout.LastUpdatedAt ?? DateTime.Now;

        // Optional: Update IsDeleted/DeletedAt if needed
        existingWorkout.IsDeleted = workout.IsDeleted;
        existingWorkout.DeletedAt = workout.DeletedAt;

        // Replace exercises
        workoutContext.WorkoutExercises.RemoveRange(existingWorkout.WorkoutExercises);
        existingWorkout.WorkoutExercises = workout.WorkoutExercises;

        // Save changes
        await workoutContext.SaveChangesAsync().ConfigureAwait(false);

        return existingWorkout;
    }

    /// <summary>
    /// Soft-deletes the workout for the given WorkoutId.
    /// </summary>
    /// <param name="workoutId">Workout ID</param>
    /// <exception cref="KeyNotFoundException">If workout does not exist</exception>
    public async Task DeleteWorkoutAsync(int workoutId)
    {
        Workout? workout = await workoutContext.Workouts.FindAsync(workoutId);

        if (workout is null)
        {
            throw new KeyNotFoundException($"Workout with ID {workoutId} not found.");
        }

        if (workout.IsDeleted == true)
        {
            // Already soft-deleted
            return;
        }

        workout.IsDeleted = true;
        workout.DeletedAt = DateTime.Now;
        
        await workoutContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets workout by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Workout> GetWorkoutByIdAsync(int id)
    {
        try
        {
            Workout? result = await workoutContext.
                Workouts.Include(w => w.WorkoutExercises)
                .FirstOrDefaultAsync(w => w.WorkoutId == id && w.IsDeleted == false)
                .ConfigureAwait(false);
            if (result == null)
            {
                throw new KeyNotFoundException("Workout not found or deleted.");
            }
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error occured while getting workout.");
            throw;
        }
    }
    
    /// <summary>
    /// Creates workout schedule
    /// </summary>
    /// <param name="workoutSchedule"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<WorkoutSchedule> AddScheduleWorkoutAsync(WorkoutSchedule workoutSchedule)
    {
        try
        {
            await workoutContext.WorkoutSchedules.AddAsync(workoutSchedule).ConfigureAwait(false);
            await workoutContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error occured while schedule workout.");
            throw;
        }
        return workoutSchedule;
    }
    
    /// <summary>
    /// Gets upcoming schedules for the user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<WorkoutSchedule>> GetUpcomingSchedulesAsync(int userId)
    {
        try
        {
            return await workoutContext.WorkoutSchedules
                .Include(ws => ws.Workout)
                .ThenInclude(w => w.WorkoutExercises)
                .ThenInclude(w => w.Exercise)
                .Where(ws => ws.ScheduledDate >= DateTime.Now
                             && ws.Status == "Pending"
                             && ws.Workout.UserId == userId
                             && ws.Workout.IsDeleted == false)
                .OrderBy(ws => ws.ScheduledDate)
                .ToListAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured while getting upcoming schedules.");
            throw;
        }
    }
    
    /// <summary>
    /// All completed workout list
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<Workout>> GetCompletedWorkoutsAsync(int userId)
    {
        return await workoutContext.Workouts
            .Include(w => w.WorkoutExercises)
            .ThenInclude(we => we.Exercise)
            .Include(w => w.WorkoutSchedules)
            .Where(w => w.UserId == userId && w.IsDeleted == false  
                        && w.WorkoutSchedules.Any(s => s.Status == "Completed"))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Updates status of workout schedule
    /// </summary>
    /// <param name="scheduleId"></param>
    /// <param name="userId"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public async Task UpdateWorkoutScheduleStatusAsync(int scheduleId, int userId, string newStatus)
    {
        try
        {
            WorkoutSchedule? schedule = await workoutContext.WorkoutSchedules
                .Include(ws => ws.Workout)
                .FirstOrDefaultAsync(ws => ws.WorkoutScheduleId == scheduleId && ws.Workout.UserId == userId && ws.Workout.IsDeleted == false)
                .ConfigureAwait(false) ?? throw new KeyNotFoundException($"Workout schedule with ID {scheduleId} not found.");
        
            schedule.Status = newStatus;
            await workoutContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error occured while updating workout schedule.");
            throw;
        }
    }
}