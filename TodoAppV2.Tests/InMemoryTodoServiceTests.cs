using TodoAppV2.Models;
using TodoAppV2.Services;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace TodoAppV2.Tests
{
    public class InMemoryTodoServiceTests
    {
        // Helper to create a fresh service instance for each test
        private InMemoryTodoService CreateService()
        {
            return new InMemoryTodoService();
        }

        // =======================================================
        // I. READ (R) OPERATIONS - 3 TESTS
        // =======================================================

        [Fact]
        public async Task GetAllAsync_ReturnsInitialSeedItems()
        {
            // Arrange
            var service = CreateService();

            // Act
            var items = await service.GetAllAsync();

            // Assert
            // Checks if at least the initial seed data (3 items) is present
            Assert.True(items.Count() >= 3);
            Assert.Contains(items, t => t.Title == "Design API");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectItem()
        {
            // Arrange
            var service = CreateService();
            int existingId = 1; // Assuming 'Design API' is ID 1

            // Act
            var item = await service.GetByIdAsync(existingId);

            // Assert
            Assert.NotNull(item);
            Assert.Equal(existingId, item.Id);
            Assert.Equal("Design API", item.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullForNonExistentId()
        {
            // Arrange
            var service = CreateService();
            int nonExistentId = 9999;

            // Act
            var item = await service.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(item);
        }

        // =======================================================
        // II. CREATE (C) OPERATIONS - 3 TESTS
        // =======================================================

        [Fact]
        public async Task AddAsync_AddsNewItemWithCorrectProperties()
        {
            // Arrange
            var service = CreateService();
            var newItem = new TodoItem { Title = "Task to Add", Priority = 5, Status = TodoStatus.NotStarted };

            // Act
            var addedItem = await service.AddAsync(newItem);

            // Assert
            Assert.True(addedItem.Id > 0);
            Assert.Equal("Task to Add", addedItem.Title);
            Assert.Equal(5, addedItem.Priority);
            Assert.Equal(TodoStatus.NotStarted, addedItem.Status);
        }

        [Fact]
        public async Task ExistsWithNameAsync_ReturnsTrueForDuplicateName()
        {
            // Arrange
            var service = CreateService();
            // 'Design API' is an initial seed item
            string duplicateTitle = "Design API";

            // Act
            var exists = await service.ExistsWithNameAsync(duplicateTitle);

            // Assert
            Assert.True(exists);
        }

        [Theory]
        [InlineData("Design API", true)]      // Case-insensitive duplicate
        [InlineData("  design api  ", true)]  // Trimmed, case-insensitive duplicate
        [InlineData("Design   API", true)]    // Multiple spaces duplicate
        [InlineData("Unique Task", false)]    // Not a duplicate
        public async Task ExistsWithNameAsync_HandlesNormalizationCorrectly(string title, bool expected)
        {
            // Arrange
            var service = CreateService(); // Uses seed data with "Design API"

            // Act
            var exists = await service.ExistsWithNameAsync(title);

            // Assert
            Assert.Equal(expected, exists);
        }

        // =======================================================
        // III. UPDATE (U) OPERATIONS - 3 TESTS
        // =======================================================

        [Fact]
        public async Task UpdateAsync_UpdatesAllPropertiesCorrectly()
        {
            // Arrange
            var service = CreateService();
            var itemToUpdate = new TodoItem
            {
                Id = 2, // 'Implement Services'
                Title = "Updated Title",
                Priority = 10,
                Status = TodoStatus.Completed
            };

            // Act
            var updatedItem = await service.UpdateAsync(itemToUpdate);

            // Assert
            Assert.NotNull(updatedItem);
            Assert.Equal("Updated Title", updatedItem.Title);
            Assert.Equal(10, updatedItem.Priority);
            Assert.Equal(TodoStatus.Completed, updatedItem.Status);

            // Verify the change is persisted
            var persistedItem = await service.GetByIdAsync(2);
            Assert.Equal("Updated Title", persistedItem.Title);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNullForNonExistentItem()
        {
            // Arrange
            var service = CreateService();
            var nonExistentItem = new TodoItem { Id = 9998, Title = "No where", Priority = 1 };

            // Act
            var result = await service.UpdateAsync(nonExistentItem);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsWithNameAsync_AllowsOriginalTitleWhenEditing()
        {
            // Arrange
            var service = CreateService();
            int idToEdit = 2;
            string originalTitle = "Implement Services"; // Title of ID 2

            // Act
            // Check if the title exists, excluding item ID 2
            var exists = await service.ExistsWithNameAsync(originalTitle, idToEdit);

            // Assert
            // Should return false because the only match is the item being edited itself
            Assert.False(exists);
        }

        // =======================================================
        // IV. DELETE (D) OPERATIONS - 7 TESTS
        // =======================================================

        [Fact]
        public async Task DeleteCompletedAsync_SucceedsForCompletedTask()
        {
            // Arrange
            var service = CreateService();
            // Item 1 is seeded as Completed: { Id = 1, Title = "Design API", Status = TodoStatus.Completed }
            int completedId = 1;

            // Act
            var isDeleted = await service.DeleteCompletedAsync(completedId);

            // Assert
            Assert.True(isDeleted);
            var deletedItem = await service.GetByIdAsync(completedId);
            Assert.Null(deletedItem); // Item must be gone
        }

        [Fact]
        public async Task DeleteCompletedAsync_FailsForNotStartedTask()
        {
            // Arrange
            var service = CreateService();
            // Item 3 is seeded as NotStarted: { Id = 3, Title = "Write Unit Tests", Status = TodoStatus.NotStarted }
            int notStartedId = 3;

            // Act
            var isDeleted = await service.DeleteCompletedAsync(notStartedId);

            // Assert
            Assert.False(isDeleted);
            var item = await service.GetByIdAsync(notStartedId);
            Assert.NotNull(item); // Item must still exist
        }

        [Fact]
        public async Task DeleteCompletedAsync_FailsForInProgressTask()
        {
            // Arrange
            var service = CreateService();
            // Item 2 is seeded as InProgress: { Id = 2, Title = "Implement Services", Status = TodoStatus.InProgress }
            int inProgressId = 2;

            // Act
            var isDeleted = await service.DeleteCompletedAsync(inProgressId);

            // Assert
            Assert.False(isDeleted);
            var item = await service.GetByIdAsync(inProgressId);
            Assert.NotNull(item); // Item must still exist
        }

        [Fact]
        public async Task DeleteCompletedAsync_FailsForNonExistentId()
        {
            // Arrange
            var service = CreateService();
            int nonExistentId = 9996;

            // Act
            var isDeleted = await service.DeleteCompletedAsync(nonExistentId);

            // Assert
            Assert.False(isDeleted);
        }

        // -------------------------------------------------------
        // COMPLEX DELETE SCENARIO TESTS
        // -------------------------------------------------------

        [Fact]
        public async Task DeleteCompletedAsync_SucceedsAfterStatusUpdate()
        {
            // Arrange
            var service = CreateService();
            // Start with an InProgress task (ID 2)
            var itemToComplete = new TodoItem { Id = 2, Title = "Implement Services", Priority = 2, Status = TodoStatus.Completed };

            // 1. Update the status to Completed
            await service.UpdateAsync(itemToComplete);

            // Act
            // 2. Attempt deletion
            var isDeleted = await service.DeleteCompletedAsync(2);

            // Assert
            Assert.True(isDeleted); // Deletion must succeed
            Assert.Null(await service.GetByIdAsync(2));
        }

        [Fact]
        public async Task DeleteCompletedAsync_FailsIfAttemptedTwice()
        {
            // Arrange
            var service = CreateService();
            int completedId = 1;

            // Act 1: Successful deletion
            var isDeletedFirstTime = await service.DeleteCompletedAsync(completedId);

            // Act 2: Attempt deletion again
            var isDeletedSecondTime = await service.DeleteCompletedAsync(completedId);

            // Assert
            Assert.True(isDeletedFirstTime);
            Assert.False(isDeletedSecondTime); // Must fail the second time (because the item is gone)
        }

        [Fact]
        public async Task GetAllAsync_ShowsCorrectOrderAfterDeletion()
        {
            // Arrange
            var service = CreateService();
            // Delete the highest priority item (ID 1, Priority 1, Completed)
            await service.DeleteCompletedAsync(1);

            // Act
            var items = await service.GetAllAsync();

            // Assert
            Assert.DoesNotContain(items, t => t.Id == 1);

            // Verify new top item is the next highest priority item (ID 2, Priority 2)
            Assert.Equal(2, items.First().Id);
        }
    }
}