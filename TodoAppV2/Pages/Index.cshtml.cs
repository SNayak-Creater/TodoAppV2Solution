using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoAppV2.Models;
using TodoAppV2.Services;

namespace TodoAppV2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITodoService _service;

        public IndexModel(ITodoService service)
        {
            _service = service;
        }

        public IEnumerable<TodoItem> TodoItems { get; set; } = Enumerable.Empty<TodoItem>();

        // BindProperty for the Add form input
        [BindProperty]
        public TodoItem NewItem { get; set; } = new TodoItem();

        public async Task OnGetAsync()
        {
            TodoItems = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            // 1. Server-Side Validation (Data Annotations)
            if (!ModelState.IsValid)
            {
                TodoItems = await _service.GetAllAsync();
                return Page(); // Redisplay page with errors
            }

            // 2. Server-Side Validation (Unique Name Business Rule)
            if (await _service.ExistsWithNameAsync(NewItem.Title))
            {
                ModelState.AddModelError("NewItem.Title", "A task with this name already exists.");
                TodoItems = await _service.GetAllAsync();
                return Page();
            }

            // Set default status (not needed if set in Model, but good practice)
            NewItem.Status = TodoStatus.NotStarted;

            await _service.AddAsync(NewItem);

            return RedirectToPage(); // Redirect back to Index (re-runs OnGet)
        }

        // Handler to delete *completed* tasks
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool success = await _service.DeleteCompletedAsync(id);

            if (!success)
            {
                // Optional: add a temporary message if deletion failed due to status
                TempData["DeleteError"] = $"Cannot delete task ID {id}. Only completed tasks can be deleted.";
            }

            return RedirectToPage();
        }

        // NEW HANDLER for Remote Validation (Client-side)
        // This is called via AJAX by the client-side validation scripts.
        public async Task<JsonResult> OnGetValidateName(string title, int id)
        {
            // The 'id' parameter is 0 when adding a new item.
            // The 'title' parameter is the value typed in the form.

            bool exists = await _service.ExistsWithNameAsync(title, id);

            if (exists)
            {
                // Return false (a JSON object with the error message) if the name exists.
                return new JsonResult("A task with this name already exists.");
            }

            // Return true if the name is unique.
            return new JsonResult(true);
        }
    }
}
