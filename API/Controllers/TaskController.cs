using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dbProject.Data;
using dbProject.Models;
using dbProject.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = dbProject.Models.Task;

namespace dbProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
        {
            return await _context.Tasks.Include(t => t.Todos).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Task>> GetTask(int id)
        {
            var task = await _context.Tasks.Include(t => t.Todos)
                                            .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        [HttpPost]
        public async Task<ActionResult<Task>> PostTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (string.IsNullOrWhiteSpace(createTaskDto.Name) || 
                createTaskDto.TodoIds == null || 
                !createTaskDto.TodoIds.Any())
            {
                return BadRequest("Task name and at least one Todo ID are required.");
            }

            var task = new Task
            {
                Name = createTaskDto.Name,
                Todos = new List<Todo>()
            };

            foreach (var todoId in createTaskDto.TodoIds)
            {
                var todoExists = await _context.Todos.AnyAsync(t => t.TodoId == todoId);
                if (!todoExists)
                {
                    return NotFound($"Todo with ID {todoId} not found.");
                }

                var todo = await _context.Todos.FindAsync(todoId);
                task.Todos.Add(todo);
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Task task)
        {
            if (id != task.TaskId)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
    }
}
