using TodoAppV2.Models;

namespace TodoAppV2.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoItem>> GetAllAsync();
        Task<TodoItem?> GetByIdAsync(int id);

        // New: Check for uniqueness before adding/updating
        Task<bool> ExistsWithNameAsync(string title, int excludeId = 0);

        Task<TodoItem> AddAsync(TodoItem item);
        Task<TodoItem?> UpdateAsync(TodoItem item);

        // Modified: Only allow deletion if the task is completed
        Task<bool> DeleteCompletedAsync(int id);
    }
}