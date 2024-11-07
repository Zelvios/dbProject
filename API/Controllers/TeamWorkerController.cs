using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dbProject.Data;
using dbProject.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamWorkerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamWorkerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TeamWorker>> PostTeamWorker([FromBody] TeamWorker teamWorker)
        {
            if (teamWorker == null)
            {
                return BadRequest("TeamWorker data is required.");
            }

            var teamExists = await _context.Teams.AnyAsync(t => t.TeamId == teamWorker.TeamId);
            var workerExists = await _context.Workers.AnyAsync(w => w.WorkerId == teamWorker.WorkerId);

            if (!teamExists)
            {
                return NotFound($"Team with ID {teamWorker.TeamId} not found.");
            }

            if (!workerExists)
            {
                return NotFound($"Worker with ID {teamWorker.WorkerId} not found.");
            }

            var existingEntry = await _context.TeamWorkers
                .AnyAsync(tw => tw.TeamId == teamWorker.TeamId && tw.WorkerId == teamWorker.WorkerId);
    
            if (existingEntry)
            {
                return BadRequest($"TeamWorker with Team ID {teamWorker.TeamId} and Worker ID {teamWorker.WorkerId} already exists.");
            }

            _context.TeamWorkers.Add(teamWorker);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostTeamWorker), new { id = teamWorker.TeamWorkerId }, teamWorker);
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamWorker>>> GetTeamWorkers()
        {
            return await _context.TeamWorkers.ToListAsync();
        }

        [HttpDelete("{teamId}/{workerId}")]
        public async Task<IActionResult> DeleteTeamWorker(int teamId, int workerId)
        {
            var teamWorker = await _context.TeamWorkers
                .FirstOrDefaultAsync(tw => tw.TeamId == teamId && tw.WorkerId == workerId);

            if (teamWorker == null)
            {
                return NotFound();
            }

            _context.TeamWorkers.Remove(teamWorker);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}