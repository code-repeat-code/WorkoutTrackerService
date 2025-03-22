using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Models.ReportModels;
using WorkoutTrackerServices.Services.Interfaces;

namespace WorkoutTrackerServices.Controllers;

[ApiController]
[Route("/v1/workout")]
public class WorkoutController(IWorkoutService workoutService, ILogger<WorkoutController> logger, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Creates a new workout with multiple exercises
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkoutSaveResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<WorkoutSaveResponseDto>), 500)]
    public async Task<ActionResult<WorkoutSaveResponseDto>> CreateWorkout([FromBody] WorkoutRequestDto request)
    {
        try
        {
            // Extract userId from token
            request.UserId = GetUserIdFromToken();
            Workout workout = await workoutService.AddWorkoutAsync(request).ConfigureAwait(false);

            ApiResponse<WorkoutSaveResponseDto> result = new()
            {
                Data = [
                    new WorkoutSaveResponseDto
                    {
                        WorkoutId = workout.WorkoutId,
                        WorkoutName = workout.WorkoutName
                    }
                ]
            };
            return CreatedAtAction(nameof(CreateWorkout), result);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized attempt no userid is present in token.");
            return StatusCode(StatusCodes.Status401Unauthorized,"You are not authorized to create this workout.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating workout.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the workout.");
        }
    }
    
    /// <summary>
    /// Gets all exercise reference data.
    /// </summary>
    [HttpGet("exercises")]
    [ProducesResponseType(typeof(ApiResponse<ExerciseResponseDto>), 200)]
    public async Task<ActionResult<ApiResponse<ExerciseResponseDto>>> GetExerciseRefData()
    {
        try
        {
            IList<Exercise> exercises = await workoutService.GetAllExerciseRefDataAsync();
            mapper.Map<IList<ExerciseResponseDto>>(exercises);
            ApiResponse<Exercise> response = new ApiResponse<Exercise>
            {
                Data = exercises
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching exercise reference data.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving exercise data.");
        }
    }
    
    /// <summary>
    /// Updates an existing workout. Only the owner can update.
    /// </summary>
    /// <param name="workoutId">WorkoutId</param>
    /// <param name="request">Workout update request</param>
    /// <returns>Updated workout info</returns>
    [HttpPut("{workoutId:int}")]
    [ProducesResponseType(typeof(ApiResponse<WorkoutSaveResponseDto>), 200)]
    [ProducesResponseType(typeof(string), 403)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<ApiResponse<WorkoutSaveResponseDto>>> UpdateWorkout(int workoutId, [FromBody] WorkoutUpdateRequestDto request)
    {
        try
        {
            request.UserId = GetUserIdFromToken();
            request.WorkoutId = workoutId;
            Workout updatedWorkout = await workoutService.UpdateWorkoutAsync(request);

            ApiResponse<WorkoutSaveResponseDto> response = new ApiResponse<WorkoutSaveResponseDto>
            {
                Data = [
                    new WorkoutSaveResponseDto
                    {
                        WorkoutId = updatedWorkout.WorkoutId,
                        WorkoutName = updatedWorkout.WorkoutName
                    }
                ]
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized update attempt.");
            return StatusCode(StatusCodes.Status401Unauthorized, "You are not authorized to update this workout.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Workout not found.");
            return StatusCode(StatusCodes.Status404NotFound,ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during workout update.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
    
    /// <summary>
    /// Soft-deletes a workout by its ID. Only the owner of the workout can delete it.
    /// </summary>
    /// <param name="workoutId">The ID of the workout to delete.</param>
    /// <returns>No content on success. Returns 403 if unauthorized, 404 if not found.</returns>
    [HttpDelete("{workoutId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteWorkout(int workoutId)
    {
        try
        {
            await workoutService.DeleteWorkoutAsync(workoutId, GetUserIdFromToken());
            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized delete attempt.");
            return StatusCode(StatusCodes.Status401Unauthorized,"You are not authorized to delete this workout.");
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Workout not found.");
            return StatusCode(StatusCodes.Status404NotFound,ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during workout deletion.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
    
    /// <summary>
    /// Schedules an existing workout for a specific date and time. Automatically marks the status as Pending.
    /// </summary>
    /// <param name="workoutScheduleRequestDto">The workout schedule request containing WorkoutId and ScheduledDate.</param>
    /// <returns>The scheduled workout details.</returns>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ApiResponse<WorkoutScheduleResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<WorkoutScheduleResponseDto>>> ScheduleWorkout([FromBody] WorkoutScheduleRequestDto workoutScheduleRequestDto)
    {
        try
        {
            WorkoutScheduleResponseDto schedule = await workoutService.AddWorkoutScheduleAsync(workoutScheduleRequestDto, GetUserIdFromToken()).ConfigureAwait(false);
            return StatusCode(StatusCodes.Status201Created,new ApiResponse<WorkoutScheduleResponseDto> { Data = [schedule] });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule workout.");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    /// <summary>
    /// Retrieves a list of upcoming scheduled workouts for the current user, sorted by scheduled date and time.
    /// </summary>
    /// <returns>List of upcoming scheduled workouts including workout and exercise details.</returns>
    [HttpGet("schedules/upcoming")]
    [ProducesResponseType(typeof(ApiResponse<WorkoutScheduleDetailedResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<WorkoutScheduleDetailedResponseDto>>> GetUpcomingSchedules()
    {
        try
        {
            List<WorkoutScheduleDetailedResponseDto> schedules = await workoutService.GetUpcomingSchedulesAsync(GetUserIdFromToken());
            return StatusCode(StatusCodes.Status200OK,new ApiResponse<WorkoutScheduleDetailedResponseDto> { Data = schedules });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get GetUpcomingSchedules workout.");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    
    /// <summary>
    /// Generates a report of all completed workouts for the user.
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(ApiResponse<WorkoutReportResponseDto>), 200)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<ApiResponse<WorkoutReportResponseDto>>> GenerateReports()
    {
        try
        {
            List<WorkoutReportResponseDto> reports = await workoutService.GenerateWorkoutReportsAsync(GetUserIdFromToken());
            return StatusCode(StatusCodes.Status200OK,new ApiResponse<WorkoutReportResponseDto> { Data = reports });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate workout reports.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating workout reports.");
        }
    }
    /// <summary>
    /// Updates the status of a scheduled workout (e.g., Pending → Completed).
    /// </summary>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <param name="dto">The new status to apply.</param>
    [HttpPatch("schedules/{scheduleId:int}/status")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateWorkoutScheduleStatus(int scheduleId, [FromBody] UpdateWorkoutScheduleStatusDto dto)
    {
        try
        {
            await workoutService.UpdateWorkoutScheduleStatusAsync(scheduleId, GetUserIdFromToken(), dto.Status).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return StatusCode(StatusCodes.Status404NotFound,ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating workout schedule status.");
            return StatusCode(500, "An error occurred while updating schedule status.");
        }
    }
    
    /// <summary>
    /// Gets user id from token
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private int GetUserIdFromToken()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing UserId in token.");
        }
        return userId;
    }
}