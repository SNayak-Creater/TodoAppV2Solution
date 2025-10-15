using TodoAppV2.Models;
using System.Text.RegularExpressions; // Required for replacing multiple spaces

namespace TodoAppV2.Services
{
    public class InMemoryTodoService : ITodoService
    {
        private string NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            // 1. Trim leading and trailing spaces
            string trimmedTitle = title.Trim();

            // 2. Replace sequences of internal whitespace (tabs, multiple spaces) with a single space
            // This ensures "Task  A" is treated the same as "Task A".
            string normalizedTitle = Regex.Replace(trimmedTitle, @"\s+", " ");

            // 3. Convert to a consistent case (e.g., lowercase) for case-insensitive comparison later
            return normalizedTitle.ToLowerInvariant();
        }
        // Initial Seed Data
        private static readonly List<TodoItem> _todoItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Design API", Priority = 1, Status = TodoStatus.Completed },
            new TodoItem { Id = 2, Title = "Implement Services", Priority = 2, Status = TodoStatus.InProgress },
            new TodoItem { Id = 3, Title = "Write Unit Tests", Priority = 3, Status = TodoStatus.NotStarted }
        };
        private static int _nextId = 4;

        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await Task.FromResult(_todoItems.OrderBy(t => t.Priority).AsEnumerable());
        }

        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await Task.FromResult(_todoItems.FirstOrDefault(t => t.Id == id));
        }

        // Business Rule: Check for uniqueness (case-insensitive)
        //public async Task<bool> ExistsWithNameAsync(string title, int excludeId = 0)
        //{
        //    return await Task.FromResult(
        //        _todoItems.Any(t =>
        //            t.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && t.Id != excludeId
        //        )
        //    );
        //}
        public async Task<bool> ExistsWithNameAsync(string title, int excludeId = 0)
        {
            // Normalize the title the user is trying to check/add
            string normalizedInputTitle = NormalizeTitle(title);

            if (string.IsNullOrEmpty(normalizedInputTitle))
            {
                // If the normalized title is empty, it shouldn't be allowed as a duplicate
                // (The [Required] attribute handles the empty case, but this is a safety net)
                return false;
            }

            // Check against all existing items (excluding the one being edited, if excludeId > 0)
            return await Task.FromResult(
                _todoItems.Any(t =>
                    t.Id != excludeId &&
                    // Compare the normalized input title against the normalized stored title
                    NormalizeTitle(t.Title).Equals(normalizedInputTitle)
                )
            );
        }
        public async Task<TodoItem> AddAsync(TodoItem item)
        {
            item.Id = _nextId++;
            _todoItems.Add(item);
            return await Task.FromResult(item);
        }

        public async Task<TodoItem?> UpdateAsync(TodoItem item)
        {
            var existingItem = _todoItems.FirstOrDefault(t => t.Id == item.Id);
            if (existingItem == null)
            {
                return null;
            }

            existingItem.Title = item.Title;
            existingItem.Priority = item.Priority;
            existingItem.Status = item.Status;

            return await Task.FromResult(existingItem);
        }

        // Business Rule: Only delete if the task is completed
        public async Task<bool> DeleteCompletedAsync(int id)
        {
            var existingItem = _todoItems.FirstOrDefault(t => t.Id == id);

            if (existingItem == null || existingItem.Status != TodoStatus.Completed)
            {
                // Return false if item not found OR not completed
                return await Task.FromResult(false);
            }

            _todoItems.Remove(existingItem);
            return await Task.FromResult(true);
        }
    }
}