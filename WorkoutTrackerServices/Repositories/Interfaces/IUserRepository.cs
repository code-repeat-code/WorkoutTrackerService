using WorkoutTrackerServices.Entities;

namespace WorkoutTrackerServices.Repositories.Interfaces;

public interface IUserRepository
{
    public Task AddAsync(User user);
    public Task<User?> GetByIdAsync(int userId);
    public Task<User?> GetByEmailAsync(string email);
    public Task UpdateUser(User user);
}