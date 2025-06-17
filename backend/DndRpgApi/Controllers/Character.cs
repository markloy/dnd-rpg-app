using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DndRpgApi.Data;
using DndRpgApi.Models;

namespace DndRpgApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CharacterController> _logger;
        
        public CharacterController(ApplicationDbContext context, ILogger<CharacterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET /api/character - Get all characters
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Character>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharacters()
        {
            try
            {
                _logger.LogInformation("GetCharacters endpoint called");
                
                var characters = await _context.Characters
                    .OrderBy(c => c.Name)  // Add sorting for better UX
                    .ToListAsync();
                
                _logger.LogInformation("Retrieved {Count} characters", characters.Count);
                return Ok(characters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving characters");
                return StatusCode(500, "An error occurred while retrieving characters");
            }
        }

        // GET /api/character/{id} - Get specific character
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Character), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Character>> GetCharacter(int id)
        {
            try
            {
                _logger.LogInformation("GetCharacter endpoint called for ID: {CharacterId}", id);
                
                if (id <= 0)
                {
                    return BadRequest("Character ID must be greater than 0");
                }
                
                var character = await _context.Characters.FindAsync(id);
                
                if (character == null)
                {
                    _logger.LogWarning("Character with ID {CharacterId} not found", id);
                    return NotFound($"Character with ID {id} not found");
                }
                
                return Ok(character);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving character {CharacterId}", id);
                return StatusCode(500, "An error occurred while retrieving the character");
            }
        }

        // POST /api/character - Create new character
        [HttpPost]
        [ProducesResponseType(typeof(Character), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Character>> CreateCharacter([FromBody] Character character)
        {
            try
            {
                _logger.LogInformation("CreateCharacter endpoint called");
                
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                if (character == null)
                {
                    return BadRequest("Character data is required.");
                }

                // Set default values for new character
                character.Level = 1;
                character.Health = character.MaxHealth > 0 ? character.MaxHealth : 100;
                character.MaxHealth = character.MaxHealth > 0 ? character.MaxHealth : 100;
                character.Experience = 0;
                character.ExperienceToNextLevel = 300; // Standard D&D progression
                character.TotalExperience = 0;
                character.Gold = 100; // Starting gold
                character.UserId = "temp-user"; // TODO: Replace with actual user ID 
                character.CreatedAt = DateTime.UtcNow;
                character.UpdatedAt = DateTime.UtcNow;

                // Set default stats if not provided
                if (character.Strength == 0) character.Strength = 16;
                if (character.Dexterity == 0) character.Dexterity = 14;
                if (character.Constitution == 0) character.Constitution = 15;
                if (character.Intelligence == 0) character.Intelligence = 12;
                if (character.Wisdom == 0) character.Wisdom = 13;
                if (character.Charisma == 0) character.Charisma = 10;

                _context.Characters.Add(character);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created character {CharacterName} with ID {CharacterId}", 
                    character.Name, character.Id);
                
                return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating character");
                return StatusCode(500, "An error occurred while creating the character");
            }
        }

        // PUT /api/character/{id} - Update character
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Character), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Character>> UpdateCharacter(int id, [FromBody] Character character)
        {
            try
            {
                _logger.LogInformation("UpdateCharacter endpoint called for ID: {CharacterId}", id);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                if (id <= 0)
                {
                    return BadRequest("Character ID must be greater than 0");
                }
                
                if (character == null)
                {
                    return BadRequest("Character data is required");
                }
                
                if (id != character.Id)
                {
                    return BadRequest("Character ID mismatch.");
                }

                // Check if character exists
                if (!await CharacterExistsAsync(id))
                {
                    return NotFound($"Character with ID {id} not found");
                }

                // Update timestamp
                character.UpdatedAt = DateTime.UtcNow;

                _context.Entry(character).State = EntityState.Modified;
                
                // Preserve CreatedAt and UserId from original
                _context.Entry(character).Property(x => x.CreatedAt).IsModified = false;
                _context.Entry(character).Property(x => x.UserId).IsModified = false;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated character {CharacterName} with ID {CharacterId}", 
                    character.Name, character.Id);
                
                return Ok(character);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await CharacterExistsAsync(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error updating character {CharacterId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating character {CharacterId}", id);
                return StatusCode(500, "An error occurred while updating the character");
            }
        }

        // DELETE /api/character/{id} - Delete character
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            try
            {
                _logger.LogInformation("DeleteCharacter endpoint called for ID: {CharacterId}", id);
                
                if (id <= 0)
                {
                    return BadRequest("Character ID must be greater than 0");
                }
                
                var character = await _context.Characters.FindAsync(id);
                if (character == null)
                {
                    _logger.LogWarning("Character with ID {CharacterId} not found for deletion", id);
                    return NotFound($"Character with ID {id} not found");
                }

                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Deleted character {CharacterName} with ID {CharacterId}", 
                    character.Name, character.Id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting character {CharacterId}", id);
                return StatusCode(500, "An error occurred while deleting the character");
            }
        }

        // Helper method - made async for better performance
        private async Task<bool> CharacterExistsAsync(int id)
        {
            return await _context.Characters.AnyAsync(e => e.Id == id);
        }
    }
}