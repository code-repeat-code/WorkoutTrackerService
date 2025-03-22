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
    
    /// <summary>
    /// Gets user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            logger.LogInformation("Fetching user with email: {Email}", email);

            var user = await workoutContext.Users.FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);

            if (user == null)
            {
                logger.LogWarning("User with Email: {Email} not found", email);
            }

            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching user with username: {Email}", email);
            throw;
        }
    }
    /// <summary>
    /// Updates users
    /// </summary>
    /// <param name="user"></param>
    public async Task UpdateUser(User user)
    {
        try
        {
            logger.LogInformation("Updating user with ID: {UserId}", user.UserId);

            workoutContext.Update(user);
            await workoutContext.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("User with ID: {UserId} updated successfully", user.UserId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while updating user with ID: {UserId}", user.UserId);
            throw;
        }
    }
}