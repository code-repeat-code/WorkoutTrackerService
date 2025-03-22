using System.Security.Claims;
using AutoMapper;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Models.ReportModels;
using WorkoutTrackerServices.Repositories.Interfaces;
using WorkoutTrackerServices.Services.Interfaces;

namespace WorkoutTrackerServices.Services;

public class WorkoutService(IWorkoutRepository workoutRepository, ILogger<WorkoutExercise> logger, IMapper mapper) : IWorkoutService
{
    private const string SCHEDULE_STATUS = "Pending";
    /// <summary>
    /// Adds Workout to 
    /// </summary>
    /// <param name="workout"></param>
    /// <returns></returns>
    public async Task<Workout> AddWorkoutAsync(WorkoutRequestDto workout)
    {
        try
        {
            // Map Workout DTO to Entity
            Workout workoutRequest = new Workout
            {
                UserId = workout.UserId,
                WorkoutName = workout.WorkoutName,
                Comment = workout.Comment, // Optional
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                WorkoutExercises = new List<WorkoutExercise>()
            };

            // Map each exercise from the request
            foreach (WorkoutExerciseRequestDto we in workout.Exercises)
            {
                WorkoutExercise workoutExercise = new WorkoutExercise
                {
                    ExerciseId = we.ExerciseId,
                    Sets = we.Sets ?? 0,
                    Repetitions = we.Repetitions ?? 0,
                    Weight = (float)(we.Weight ?? 0)
                };

                workoutRequest.WorkoutExercises.Add(workoutExercise);
            }
            // Save to database
            return await workoutRepository.AddWorkoutAsync(workoutRequest).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while adding workout in AddWorkoutAsync WorkoutService.");
            throw;
        }
    }
    
    /// <summary>
    /// Gets exercise ref data.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IList<Exercise>> GetAllExerciseRefDataAsync()
    {
        return await workoutRepository.GetAllExerciseRefDataAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Updates workout
    /// </summary>
    /// <param name="workout"></param>
    /// <returns></returns>
    public async Task<Workout> UpdateWorkoutAsync(WorkoutUpdateRequestDto workout)
    {
        try
        {
            //Get the existing workout for the incoming workoutId
            Workout existingWorkout = await workoutRepository.GetWorkoutByIdAsync(workout.WorkoutId).ConfigureAwait(false);
            // Validate if workout owner is updating it own workout
            if (existingWorkout.UserId != workout.UserId)
            {
                logger.LogWarning($"User {workout.UserId} does not match user id {existingWorkout.UserId}");
                throw new UnauthorizedAccessException("You can only update your own workouts.");
            }
            //Override the existing workout for now there is no history
            //keeping updating the same row without any audit.
            Workout workoutRequest = new Workout
            {
                WorkoutId = existingWorkout.WorkoutId,
                UserId = workout.UserId,
                WorkoutName = workout.WorkoutName,
                Comment = workout.Comment,
                CreatedAt = existingWorkout.CreatedAt,//Using real created timestamp
                LastUpdatedAt = DateTime.Now,
                IsDeleted = existingWorkout.IsDeleted,
                DeletedAt = existingWorkout.DeletedAt,
                WorkoutExercises = workout.Exercises.Select(we => new WorkoutExercise
                {
                    ExerciseId = we.ExerciseId,
                    Sets = we.Sets ?? 0,
                    Repetitions = we.Repetitions ?? 0,
                    Weight = (float)(we.Weight ?? 0)
                }).ToList()
            };
            return await workoutRepository.UpdateWorkoutAsync(workoutRequest).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while updating workout in UpdateWorkoutAsync WorkoutService.");
            throw;
        }
    }
    
    /// <summary>
    /// Soft-deletes a workout if the user is the owner.
    /// </summary>
    /// <param name="workoutId">Workout ID</param>
    /// <param name="userId">User ID from token</param>
    public async Task DeleteWorkoutAsync(int workoutId, int userId)
    {
        try
        {
            Workout workout = await workoutRepository.GetWorkoutByIdAsync(workoutId)
                              ?? throw new KeyNotFoundException($"Workout with ID {workoutId} not found.");

            if (workout.UserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot delete this workout.");
            }

            await workoutRepository.DeleteWorkoutAsync(workoutId).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while deleting workout in DeleteWorkoutAsync WorkoutService.");
            throw;
        }
    }
    
    /// <summary>
    /// Workout Schedule creation.
    /// </summary>
    /// <param name="workoutSchedule"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<WorkoutScheduleResponseDto> AddWorkoutScheduleAsync(WorkoutScheduleRequestDto workoutSchedule, int userId)
    {
        WorkoutScheduleResponseDto response;
        try
        {
            //get the workout exercise
            Workout workout = await workoutRepository.GetWorkoutByIdAsync(workoutSchedule.WorkoutId).ConfigureAwait(false);
            
            if (workout.UserId != userId)
                throw new UnauthorizedAccessException("Cannot schedule workout not owned by user.");
            
            WorkoutSchedule? mappedWorkoutSchedule = mapper.Map<WorkoutScheduleRequestDto, WorkoutSchedule>(workoutSchedule);
            //Create workout schedule
            mappedWorkoutSchedule.Status = SCHEDULE_STATUS;//setting as pending
            WorkoutSchedule dbWorkoutSchedule = await workoutRepository.AddScheduleWorkoutAsync(mappedWorkoutSchedule).ConfigureAwait(false);
            
            //Set the Response Model
            ICollection<WorkoutExercise> exercises = workout.WorkoutExercises;
            
            response = new()
            {
                WorkoutId = dbWorkoutSchedule.WorkoutId,
                WorkoutScheduleId = dbWorkoutSchedule.WorkoutScheduleId,
                ScheduledDate = dbWorkoutSchedule.ScheduledDate,
                Status = dbWorkoutSchedule.Status,
                Exercises = exercises.Select(e => new WorkoutExerciseResponseDto
                {
                    ExerciseId = e.ExerciseId,
                    Sets  = e.Sets,
                    Repetitions = e.Repetitions,
                    Weight = e.Weight
                }).ToList()
            };
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error occured while adding workout schedule in AddWorkoutScheduleAsync.");
            throw;
        }
    }
    
    /// <summary>
    /// Gets upcoming schedules for users
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<WorkoutScheduleDetailedResponseDto>> GetUpcomingSchedulesAsync(int userId)
    {
        try
        {
            var upcomingSchedules = await workoutRepository.GetUpcomingSchedulesAsync(userId).ConfigureAwait(false);
            var response = mapper.Map<List<WorkoutScheduleDetailedResponseDto>>(upcomingSchedules);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured while getting upcoming workout schedules.");
            throw;
        }
    }
    
    /// <summary>
    /// Workout report.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<WorkoutReportResponseDto>> GenerateWorkoutReportsAsync(int userId)
    {
        List<Workout> workouts = await workoutRepository.GetCompletedWorkoutsAsync(userId);
        return mapper.Map<List<WorkoutReportResponseDto>>(workouts);
    }
    /// <summary>
    /// Updates workout schedule status
    /// </summary>
    /// <param name="scheduleId"></param>
    /// <param name="userId"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task UpdateWorkoutScheduleStatusAsync(int scheduleId, int userId, string newStatus)
    {
         await workoutRepository.UpdateWorkoutScheduleStatusAsync(scheduleId, userId, newStatus).ConfigureAwait(false);
    }
}