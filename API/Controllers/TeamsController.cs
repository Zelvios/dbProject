using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dbProject.Data;
using dbProject.Models;
using dbProject.DTOs;
using Task = dbProject.Models.Task;

namespace dbProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams
                .Include(t => t.Workers)
                .Include(t => t.Tasks)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Workers)
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }

            return team;
        }
        
        [HttpGet("with-tasks")]
        public async Task<ActionResult<IEnumerable<TeamWithTasksDto>>> GetTeamsWithTasks()
        {
            var teamsWithTasks = await _context.Teams
                .Where(t => t.Tasks != null && t.Tasks.Any())
                .Include(t => t.Tasks)
                .Select(t => new TeamWithTasksDto
                {
                    TeamName = t.Name,
                    TaskNames = t.Tasks!.Select(task => task.Name).ToList()
                })
                .ToListAsync();

            return Ok(teamsWithTasks);
        }

        [HttpGet("without-task")]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeamsWithoutTask()
        {
            var teamsWithoutTasks = await _context.Teams
                .Where(t => t.Tasks != null && !t.Tasks.Any())
                .Include(t => t.Workers)
                .ToListAsync();

            return Ok(teamsWithoutTasks);
        }
        
        [HttpGet("all-teams-and-tasks")]
        public async Task<ActionResult<IEnumerable<TeamWithTasksDto>>> GetAllTeamsAndTasks()
        {
            var allTeamsWithTasks = await _context.Teams
                .Select(t => new TeamWithTasksDto
                {
                    TeamName = t.Name,
                    TaskNames = t.Tasks!.Select(task => task.Name).ToList()
                })
                .ToListAsync();

            return Ok(allTeamsWithTasks);
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamId == id);
        }

        [HttpPost]
        public async Task<ActionResult<Team>> PostTeam([FromBody] CreateTeamDto createTeamDto)
        {
            if (!createTeamDto.WorkerIds.Any())
            {
                return BadRequest("At least one worker ID must be provided.");
            }

            var team = new Team
            {
                Name = createTeamDto.Name,
                Tasks = new List<Task>()
            };

            foreach (var workerId in createTeamDto.WorkerIds)
            {
                var workerExists = await _context.Workers.AnyAsync(w => w.WorkerId == workerId);
                if (!workerExists)
                {
                    return NotFound($"Worker with ID {workerId} not found.");
                }
            }

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            foreach (var workerId in createTeamDto.WorkerIds)
            {
                var teamWorker = new TeamWorker
                {
                    TeamId = team.TeamId,
                    WorkerId = workerId
                };
                _context.TeamWorkers.Add(teamWorker);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { id = team.TeamId }, team);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeam(int id, Team team)
        {
            if (id != team.TeamId)
            {
                return BadRequest();
            }

            _context.Entry(team).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{teamId}/tasks")]
        public async Task<IActionResult> AddTasksToTeam(int teamId, [FromBody] List<int> taskIds)
        {
            if (!taskIds.Any())
            {
                return BadRequest("At least one task ID must be provided.");
            }

            var team = await _context.Teams.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.TeamId == teamId);
    
            if (team == null)
            {
                return NotFound($"Team with ID {teamId} not found.");
            }

            foreach (var taskId in taskIds)
            {
                var taskExists = await _context.Tasks.AnyAsync(t => t.TaskId == taskId);
                if (!taskExists)
                {
                    return NotFound($"Task with ID {taskId} not found.");
                }

                if (team.Tasks != null && team.Tasks.Any(t => t.TaskId == taskId))
                {
                    return BadRequest($"Task with ID {taskId} is already assigned to the team.");
                }

                var taskToAdd = await _context.Tasks.FindAsync(taskId);
                if (taskToAdd != null) team.Tasks!.Add(taskToAdd);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("{teamId}/change-current-task")]
        public async Task<IActionResult> ChangeCurrentTaskForTeam(int teamId, [FromBody] int newTaskId)
        {
            var team = await _context.Teams
                .Include(t => t.CurrentTask)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                return NotFound($"Team with ID {teamId} not found.");
            }

            var newTask = await _context.Tasks.FindAsync(newTaskId);
            if (newTask == null)
            {
                return NotFound($"Task with ID {newTaskId} not found.");
            }

            team.CurrentTask = newTask;

            await _context.SaveChangesAsync();

            return Ok($"Current task for team {teamId} has been changed to task {newTaskId}.");
        }
        
        [HttpGet("{teamId}/completion-percentage")]
        public async Task<ActionResult<string>> GetTeamCompletionPercentage(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Tasks)!
                .ThenInclude(task => task.Todos)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                return NotFound($"Team with ID {teamId} not found.");
            }

            var totalTodos = team.Tasks?.Sum(task => task.Todos.Count) ?? 0;
            var completedTodos = team.Tasks?.Sum(task => task.Todos.Count(todo => todo.IsComplete)) ?? 0;

            if (totalTodos == 0)
            {
                return Ok("0% done (no todos available for this team)");
            }

            var completionPercentage = (double)completedTodos / totalTodos * 100;
            return Ok($"{completionPercentage:F2}% done");
        }

    }
}
