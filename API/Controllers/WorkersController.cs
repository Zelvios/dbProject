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
    public class WorkersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Worker>>> GetWorkers()
        {
            return await _context.Workers.Include(w => w.Teams).Include(w => w.Todos).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Worker>> GetWorker(int id)
        {
            var worker = await _context.Workers
                .Include(w => w.Teams)
                .Include(w => w.Todos)
                .FirstOrDefaultAsync(w => w.WorkerId == id);

            if (worker == null)
            {
                return NotFound();
            }

            return worker;
        }

        [HttpPost]
        public async Task<ActionResult<Worker>> PostWorker(CreateWorkerDto createWorkerDto)
        {
            var worker = new Worker
            {
                Name = createWorkerDto.Name,
                Teams = null,
                Todos = null 
            };

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorker), new { id = worker.WorkerId }, worker);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorker(int id, Worker worker)
        {
            if (id != worker.WorkerId)
            {
                return BadRequest();
            }

            _context.Entry(worker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{workerId}/tasks")]
        public async Task<IActionResult> AddTasksToWorker(int workerId, [FromBody] List<int> taskIds)
        {
            var worker = await _context.Workers.Include(w => w.Todos).FirstOrDefaultAsync(w => w.WorkerId == workerId);

            if (worker == null)
            {
                return NotFound($"Worker with ID {workerId} not found.");
            }

            foreach (var taskId in taskIds)
            {
                var task = await _context.Todos.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound($"Task with ID {taskId} not found.");
                }

                if (worker.Todos != null && !worker.Todos.Contains(task))
                {
                    worker.Todos.Add(task);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{workerId}/current-task")]
        public async Task<IActionResult> SetCurrentTaskForWorker(int workerId, [FromBody] int taskId)
        {
            var worker = await _context.Workers.Include(w => w.Todos).FirstOrDefaultAsync(w => w.WorkerId == workerId);

            if (worker == null)
            {
                return NotFound($"Worker with ID {workerId} not found.");
            }

            var task = await _context.Todos.FindAsync(taskId);
            if (task == null)
            {
                return NotFound($"Task with ID {taskId} not found.");
            }

            if (worker.Todos != null && !worker.Todos.Contains(task))
            {
                worker.Todos.Add(task);
            }

            worker.CurrentTodo = task;
            await _context.SaveChangesAsync();

            return Ok($"Current task for worker {workerId} has been set to task {taskId}.");
        }

        private bool WorkerExists(int id)
        {
            return _context.Workers.Any(e => e.WorkerId == id);
        }
    }
}
