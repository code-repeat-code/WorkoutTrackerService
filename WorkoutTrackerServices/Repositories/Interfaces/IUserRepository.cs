using WorkoutTrackerServices.Entities;

namespace WorkoutTrackerServices.Repositories.Interfaces;

public interface IUserRepository
{
    public Task AddAsync(User user);
    public Task<User?> GetByIdAsync(int userId);
}