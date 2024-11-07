using Microsoft.AspNetCore.Mvc;
using dbProject.Data;
using dbProject.Models;
using System.Collections.Generic;
using System.Linq;
using Task = dbProject.Models.Task;

namespace dbProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("seed-workers")]
        public IActionResult SeedWorkers()
        {
            if (_context.Teams.Any() || _context.Workers.Any())
            {
                return BadRequest("Database already seeded.");
            }

            var workers = new List<Worker>
            {
                new Worker { Name = "Steen Secher" },
                new Worker { Name = "Ejvind Møller" },
                new Worker { Name = "Konrad Sommer" },
                new Worker { Name = "Sofus Lotus" },
                new Worker { Name = "Remo Lademann" },
                new Worker { Name = "Ella Fanth" },
                new Worker { Name = "Anne Dam" }
            };

            _context.Workers.AddRange(workers);
            _context.SaveChanges();

            var frontend = new Team { Name = "Frontend" };
            var backend = new Team { Name = "Backend" };
            var testere = new Team { Name = "Testere" };

            frontend.Workers = new List<TeamWorker>
            {
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Steen Secher").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Ejvind Møller").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Konrad Sommer").WorkerId }
            };

            backend.Workers = new List<TeamWorker>
            {
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Konrad Sommer").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Sofus Lotus").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Remo Lademann").WorkerId }
            };

            testere.Workers = new List<TeamWorker>
            {
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Ella Fanth").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Anne Dam").WorkerId },
                new TeamWorker { WorkerId = workers.First(w => w.Name == "Steen Secher").WorkerId }
            };

            _context.Teams.AddRange(frontend, backend, testere);
            _context.SaveChanges();

            return Ok("Workers and teams seeded successfully.");
        }

        [HttpPost("seed-tasks")]
        public IActionResult SeedTasks()
        {
            if (_context.Tasks.Any())
            {
                return BadRequest("Tasks already seeded.");
            }

            var tasks = new List<Task>
            {
                new Task
                {
                    Name = "Produce software",
                    Todos = new List<Todo>
                    {
                        new Todo { Name = "Write Code" },
                        new Todo { Name = "Compile source" },
                        new Todo { Name = "Test program" }
                    }
                },
                new Task
                {
                    Name = "Brew coffee",
                    Todos = new List<Todo>
                    {
                        new Todo { Name = "Pour water" },
                        new Todo { Name = "Pour coffee" },
                        new Todo { Name = "Turn on" }
                    }
                }
            };

            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();

            return Ok("Tasks and todos seeded successfully.");
        }
    }
}
