using Microsoft.EntityFrameworkCore;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Repositories.Interfaces;

namespace WorkoutTrackerServices.Repositories;

public class UserRepository(WorkoutContext workoutContext,ILogger<UserRepository> logger ) : IUserRepository
{
    /// <summary>
    /// Adding user to db.
    /// </summary>
    /// <param name="user"></param>
    public async Task AddAsync(User user)
    {
        try
        {
            logger.LogInformation("Adding new user: {Username}", user.Username);

            await workoutContext.Users.AddAsync(user).ConfigureAwait(false);
            await workoutContext.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("User {Username} added successfully", user.Username);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while adding user {Username}", user.Username);
            throw;
        }
    }
    
    /// <summary>
    /// Getting user by userid.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<User?> GetByIdAsync(int userId)
    {
        try
        {
            logger.LogInformation("Fetching user with ID: {UserId}", userId);

            var user = await workoutContext.Users.FirstOrDefaultAsync(x => x.UserId == userId).ConfigureAwait(false);

            if (user != null) return user;
            logger.LogWarning("User with ID: {UserId} not found", userId);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching user with ID: {UserId}", userId);
            throw;
        }
    }
}