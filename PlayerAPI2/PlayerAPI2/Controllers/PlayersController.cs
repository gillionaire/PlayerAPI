using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerAPI2.Models;

namespace PlayerAPI2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerContext _context;

        public PlayersController(PlayerContext context)
        {
            _context = context;
        }

        // GET: api/Players
        [HttpGet]
        public IEnumerable<Player> GetPlayers([FromQuery]string lastName, [FromQuery] int teamId)
        {
            if (!String.IsNullOrEmpty(lastName))
            {
                return _context.Players.Where(p => String.Equals(p.LastName, lastName, 
                    StringComparison.OrdinalIgnoreCase));
            }
            if (teamId != 0)
            {
                var team = _context.Teams.FindAsync(teamId);
                if (team != null)
                {
                    return _context.Players.Where(p => p.TeamId == teamId); 
                }
            }
            return _context.Players;
        }

        // GET: api/Players/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            return Ok(player);
        }

        // PUT: api/Players/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayer([FromRoute] int id, [FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != player.Id)
            {
                return BadRequest();
            }

            _context.Entry(player).State = EntityState.Modified;

            if (TeamFull(player.TeamId)) {
                return BadRequest("Team is full");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
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

        // POST: api/Players
        [HttpPost]
        public async Task<IActionResult> PostPlayer([FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (TeamFull(player.TeamId))
            {
                return BadRequest("Team is full");
            }

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }

        // DELETE: api/Players/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return Ok(player);
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AddPlayerToTeam([FromRoute] int playerId, 
            [FromQuery] int teamId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return NotFound("Player not found");
            }
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("Team not found");
            }
            if (team.Players.Count >= 8)
            {
                return BadRequest("Team is full");
            }

            player.Team = team;
            await PutPlayer(player.Id, player);

            return Ok(player);
        }

        public async Task<IActionResult> RemovePlayerFromTeam([FromRoute] int playerId,
            [FromQuery] int teamId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return NotFound("Player not found");
            }
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("Team not found");
            }

            player.Team = null;
            await PutPlayer(player.Id, player);

            return Ok(player);
        }

        private bool TeamFull(int teamId)
        {
            var team = _context.Teams.Find(teamId);
            if (team == null)
                return false;
            return team.Players.Count >= 8;
        }
    }
}