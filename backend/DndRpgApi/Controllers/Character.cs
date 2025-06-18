using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DndRpgApi.Data;
using DndRpgApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

// IMPORTANT: Use alias to avoid System.Character conflict
using DndCharacter = DndRpgApi.Models.Character;

namespace DndRpgApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all character operations
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CharactersController> _logger;

        public CharactersController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<CharactersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ===== GET ALL CHARACTERS =====
        
        /// <summary>
        /// Get all characters for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DndCharacter>>> GetCharacters()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Getting characters for user {UserId}", userId);

            var characters = await _context.Characters
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(characters);
        }

        // ===== GET CHARACTER BY ID =====
        
        /// <summary>
        /// Get a specific character by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DndCharacter>> GetCharacter(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Getting character {CharacterId} for user {UserId}", id, userId);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                _logger.LogWarning("Character {CharacterId} not found for user {UserId}", id, userId);
                return NotFound($"Character with ID {id} not found");
            }

            return Ok(character);
        }

        // ===== CREATE CHARACTER =====
        
        /// <summary>
        /// Create a new character
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DndCharacter>> CreateCharacter(CreateCharacterRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Creating character '{CharacterName}' for user {UserId}", request.Name, userId);

            // Check if character name is already taken by this user
            var existingCharacter = await _context.Characters
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == request.Name);

            if (existingCharacter != null)
            {
                return BadRequest($"You already have a character named '{request.Name}'");
            }

            var character = new DndCharacter
            {
                Name = request.Name,
                Level = 1,
                Health = CalculateStartingHitPoints(request.CharacterClass, request.Constitution),
                MaxHealth = CalculateStartingHitPoints(request.CharacterClass, request.Constitution),
                Experience = 0,
                ExperienceToNextLevel = 300, // XP needed for level 2
                TotalExperience = 0,
                Gold = 100, // Starting gold
                UserId = userId,
                CharacterClass = request.CharacterClass,
                Background = request.Background,
                Race = request.Race,
                Strength = request.Strength,
                Dexterity = request.Dexterity,
                Constitution = request.Constitution,
                Intelligence = request.Intelligence,
                Wisdom = request.Wisdom,
                Charisma = request.Charisma,
                WeaponName = GetStartingWeapon(request.CharacterClass),
                WeaponDamage = GetWeaponDamage(GetStartingWeapon(request.CharacterClass)),
                WeaponAttackBonus = CalculateAttackBonus(request.CharacterClass, request.Strength, request.Dexterity),
                ArmorName = GetStartingArmor(request.CharacterClass),
                ArmorClass = CalculateArmorClass(request.CharacterClass, request.Dexterity),
                ShieldName = GetStartingShield(request.CharacterClass),
                ShieldArmorClassBonus = GetShieldBonus(request.CharacterClass),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Character '{CharacterName}' created with ID {CharacterId}", character.Name, character.Id);

            return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
        }

        // ===== UPDATE CHARACTER =====
        
        /// <summary>
        /// Update an existing character
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCharacter(int id, UpdateCharacterRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Updating character {CharacterId} for user {UserId}", id, userId);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                return NotFound($"Character with ID {id} not found");
            }

            // Update properties
            character.Name = request.Name ?? character.Name;
            character.CharacterClass = request.CharacterClass ?? character.CharacterClass;
            character.Background = request.Background ?? character.Background;
            character.Race = request.Race ?? character.Race;
            
            if (request.Strength.HasValue) character.Strength = request.Strength.Value;
            if (request.Dexterity.HasValue) character.Dexterity = request.Dexterity.Value;
            if (request.Constitution.HasValue) character.Constitution = request.Constitution.Value;
            if (request.Intelligence.HasValue) character.Intelligence = request.Intelligence.Value;
            if (request.Wisdom.HasValue) character.Wisdom = request.Wisdom.Value;
            if (request.Charisma.HasValue) character.Charisma = request.Charisma.Value;

            character.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Character {CharacterId} updated successfully", id);

            return NoContent();
        }

        // ===== DELETE CHARACTER =====
        
        /// <summary>
        /// Delete a character
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            _logger.LogInformation("Deleting character {CharacterId} for user {UserId}", id, userId);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                return NotFound($"Character with ID {id} not found");
            }

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Character '{CharacterName}' (ID: {CharacterId}) deleted", character.Name, character.Id);

            return NoContent();
        }

        // ===== COMBAT ACTIONS =====
        
        /// <summary>
        /// Add experience to a character
        /// </summary>
        [HttpPost("{id}/experience")]
        public async Task<ActionResult<DndCharacter>> AddExperience(int id, [FromBody] int experienceGained)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                return NotFound($"Character with ID {id} not found");
            }

            _logger.LogInformation("Adding {Experience} XP to character {CharacterId}", experienceGained, id);

            character.Experience += experienceGained;
            character.TotalExperience += experienceGained;

            // Check for level up
            while (character.Experience >= character.ExperienceToNextLevel && character.Level < 20)
            {
                await LevelUpCharacter(character);
            }

            character.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(character);
        }

        /// <summary>
        /// Heal a character
        /// </summary>
        [HttpPost("{id}/heal")]
        public async Task<ActionResult<DndCharacter>> HealCharacter(int id, [FromBody] int healAmount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                return NotFound($"Character with ID {id} not found");
            }

            _logger.LogInformation("Healing character {CharacterId} for {HealAmount} HP", id, healAmount);

            character.Health = Math.Min(character.Health + healAmount, character.MaxHealth);
            character.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(character);
        }

        /// <summary>
        /// Damage a character
        /// </summary>
        [HttpPost("{id}/damage")]
        public async Task<ActionResult<DndCharacter>> DamageCharacter(int id, [FromBody] int damage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
            {
                return NotFound($"Character with ID {id} not found");
            }

            _logger.LogInformation("Dealing {Damage} damage to character {CharacterId}", damage, id);

            character.Health = Math.Max(character.Health - damage, 0);
            character.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (character.Health == 0)
            {
                _logger.LogWarning("Character {CharacterName} has been reduced to 0 HP!", character.Name);
            }

            return Ok(character);
        }

        // ===== HELPER METHODS =====

        private async Task LevelUpCharacter(DndCharacter character)
        {
            _logger.LogInformation("Leveling up character {CharacterName} from level {OldLevel}", 
                character.Name, character.Level);

            character.Experience -= character.ExperienceToNextLevel;
            character.Level++;
            character.ExperienceToNextLevel = GetExperienceForLevel(character.Level + 1);

            // Increase hit points (simplified)
            var hitPointIncrease = 1 + GetAbilityModifier(character.Constitution);
            character.MaxHealth += Math.Max(1, hitPointIncrease);
            character.Health += Math.Max(1, hitPointIncrease); // Heal on level up
            
            // Add this to make it truly async (even though it's not needed now):
            await Task.CompletedTask;
        }

        private static int GetExperienceForLevel(int level)
        {
            return level switch
            {
                2 => 300,
                3 => 900,
                4 => 2700,
                5 => 6500,
                6 => 14000,
                7 => 23000,
                8 => 34000,
                9 => 48000,
                10 => 64000,
                11 => 85000,
                12 => 100000,
                13 => 120000,
                14 => 140000,
                15 => 165000,
                16 => 195000,
                17 => 225000,
                18 => 265000,
                19 => 305000,
                20 => 355000,
                _ => 0
            };
        }

        private static int GetAbilityModifier(int abilityScore)
        {
            return (abilityScore - 10) / 2;
        }

        private static int CalculateStartingHitPoints(string characterClass, int constitution)
        {
            var baseHp = characterClass.ToLower() switch
            {
                "barbarian" => 12,
                "fighter" or "paladin" or "ranger" => 10,
                "bard" or "cleric" or "druid" or "monk" or "rogue" or "warlock" => 8,
                "sorcerer" or "wizard" => 6,
                _ => 8
            };

            return baseHp + GetAbilityModifier(constitution);
        }

        private static string GetStartingWeapon(string characterClass)
        {
            return characterClass.ToLower() switch
            {
                "barbarian" => "Greataxe",
                "fighter" => "Longsword",
                "paladin" => "Longsword",
                "ranger" => "Shortbow",
                "rogue" => "Shortsword",
                "cleric" => "Mace",
                "monk" => "Quarterstaff",
                "bard" => "Rapier",
                "sorcerer" or "wizard" => "Dagger",
                "warlock" => "Light Crossbow",
                "druid" => "Scimitar",
                _ => "Longsword"
            };
        }

        private static string GetWeaponDamage(string weaponName)
        {
            return weaponName.ToLower() switch
            {
                "greataxe" => "1d12",
                "longsword" => "1d8",
                "shortbow" => "1d6",
                "shortsword" => "1d6",
                "mace" => "1d6",
                "quarterstaff" => "1d6",
                "rapier" => "1d8",
                "dagger" => "1d4",
                "light crossbow" => "1d8",
                "scimitar" => "1d6",
                _ => "1d8"
            };
        }

        private static int CalculateAttackBonus(string characterClass, int strength, int dexterity)
        {
            var proficiencyBonus = 2; // Level 1 proficiency
            var isFinesse = characterClass.ToLower() is "rogue" or "bard" or "monk";
            var abilityModifier = isFinesse ? GetAbilityModifier(dexterity) : GetAbilityModifier(strength);
            
            return proficiencyBonus + abilityModifier;
        }

        private static string GetStartingArmor(string characterClass)
        {
            return characterClass.ToLower() switch
            {
                "barbarian" or "monk" or "sorcerer" or "wizard" => "Unarmored",
                "rogue" or "ranger" => "Leather Armor",
                "bard" or "cleric" or "druid" or "warlock" => "Leather Armor",
                "fighter" or "paladin" => "Chain Mail",
                _ => "Leather Armor"
            };
        }

        private static int CalculateArmorClass(string characterClass, int dexterity)
        {
            var dexMod = GetAbilityModifier(dexterity);
            
            return characterClass.ToLower() switch
            {
                "barbarian" or "monk" or "sorcerer" or "wizard" => 10 + dexMod,
                "rogue" or "ranger" or "bard" or "cleric" or "druid" or "warlock" => 11 + dexMod,
                "fighter" or "paladin" => 16, // Chain mail
                _ => 11 + dexMod
            };
        }

        private static string GetStartingShield(string characterClass)
        {
            return characterClass.ToLower() switch
            {
                "fighter" or "paladin" or "cleric" => "Shield",
                _ => ""
            };
        }

        private static int GetShieldBonus(string characterClass)
        {
            return characterClass.ToLower() switch
            {
                "fighter" or "paladin" or "cleric" => 2,
                _ => 0
            };
        }
    }

    // ===== REQUEST MODELS =====

    public class CreateCharacterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CharacterClass { get; set; } = "Fighter";
        public string Background { get; set; } = "Soldier";
        public string Race { get; set; } = "Human";
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Constitution { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Wisdom { get; set; } = 10;
        public int Charisma { get; set; } = 10;
    }

    public class UpdateCharacterRequest
    {
        public string? Name { get; set; }
        public string? CharacterClass { get; set; }
        public string? Background { get; set; }
        public string? Race { get; set; }
        public int? Strength { get; set; }
        public int? Dexterity { get; set; }
        public int? Constitution { get; set; }
        public int? Intelligence { get; set; }
        public int? Wisdom { get; set; }
        public int? Charisma { get; set; }
    }
}