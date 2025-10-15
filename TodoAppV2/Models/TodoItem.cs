using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc; // Required for [Remote]

namespace TodoAppV2.Models
{
    public enum TodoStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public class TodoItem
    {
        public int Id { get; set; }

        // 1. Validation: Task Name is required
        [Required(ErrorMessage = "Task Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Task Name must be between 3 and 100 characters.")]
        [Display(Name = "Task Name")]
        // NEW: Remote Validation Attribute
        //[Remote(
        //    action: "ValidateName",         // Calls OnGetValidateName handler
        //    pageName: "/Index",             // Page where the handler is located
        //    HttpMethod = "GET",
        //    // The 'AdditionalFields' sends the ID, which is necessary when editing
        //    AdditionalFields = "__RequestVerificationToken,NewItem.Id,TodoItem.Id"
        //)]
        public string Title { get; set; } = string.Empty;

        // 2. Validation: Priority is a required number (int)
        [Required(ErrorMessage = "Priority is required.")]
        [Range(1, 100, ErrorMessage = "Priority must be a number between 1 and 100.")]
        public int Priority { get; set; }

        // 3. Status
        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;

        // Helper property to check completion status
        public bool IsCompleted => Status == TodoStatus.Completed;
    }
}