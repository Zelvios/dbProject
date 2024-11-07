using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dbProject.Data;
using dbProject.Models;
using dbProject.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            return await _context.Todos.ToListAsync();
        }
        
        [HttpGet("incomplete")]
        public async Task<ActionResult<IEnumerable<Todo>>> GetIncompleteTodos()
        {
            var incompleteTodos = await _context.Todos
                .Where(t => !t.IsComplete)
                .ToListAsync();

            return Ok(incompleteTodos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return todo;
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodo([FromBody] CreateTodoDto createTodoDto)
        {
            // Validate the DTO
            if (createTodoDto == null || string.IsNullOrWhiteSpace(createTodoDto.Name))
            {
                return BadRequest("Todo name is required.");
            }

            var todo = new Todo
            {
                Name = createTodoDto.Name,
                IsComplete = createTodoDto.IsComplete
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodo), new { id = todo.TodoId }, todo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo(int id, Todo todo)
        {
            if (id != todo.TodoId)
            {
                return BadRequest();
            }
            _context.Entry(todo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TodoExists(int id)
        {
            return _context.Todos.Any(e => e.TodoId == id);
        }
        
        [HttpPatch("{id}/toggle-completion")]
        public async Task<IActionResult> ToggleTodoCompletion(int id)
        {
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound($"Todo with ID {id} not found.");
            }

            todo.IsComplete = !todo.IsComplete;

            await _context.SaveChangesAsync();

            return Ok(todo);
        }

    }
}
