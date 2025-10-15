using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoAppV2.Models;
using TodoAppV2.Services;

namespace TodoAppV2.Pages
{
    public class EditModel : PageModel
    {
        private readonly ITodoService _service;

        public EditModel(ITodoService service)
        {
            _service = service;
        }

        // BindProperty for the form data (both GET and POST)
        [BindProperty]
        public TodoItem TodoItem { get; set; } = new TodoItem();

        // OnGet: Load the existing item data
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _service.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            TodoItem = item;
            return Page();
        }

        // OnPost: Handle form submission (Update)
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Server-Side Validation (Data Annotations)
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 2. Server-Side Validation (Unique Name Business Rule - excluding the current item)
            if (await _service.ExistsWithNameAsync(TodoItem.Title, TodoItem.Id))
            {
                // The key must match the property path exactly.
                ModelState.AddModelError("TodoItem.Title", "A task with this name already exists.");
                return Page();
            }

            var updatedItem = await _service.UpdateAsync(TodoItem);

            if (updatedItem == null)
            {
                return NotFound();
            }

            // Redirect back to the main list after successful update
            return RedirectToPage("./Index");
        }
    }
}
