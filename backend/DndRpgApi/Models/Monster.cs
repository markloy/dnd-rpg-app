using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DndRpgApi.Models
{
    // Monster entity based on D&D API structure
    public class Monster
    {
        [Key]
        public int Id { get; set; }

        // Unique identifier from D&D API (e.g., "goblin", "orc")
        [Required]
        [MaxLength(50)]
        public string ApiId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Challenge rating determines encounter suitability
        // Formula: CR = 0.25 * character level for solo encounters
        [Column(TypeName = "decimal(4,2)")] // Allows values like 0.25, 1.5, etc.
        public decimal ChallengeRating { get; set; }

        // Combat stats
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public int ProficiencyBonus { get; set; }

        // Damage dice string (e.g., "1d6+2", "2d8+4")
        [MaxLength(20)]
        public string DamageDice { get; set; } = string.Empty;

        // Experience points awarded when defeated
        public int ExperiencePoints { get; set; }

        // Image URL from D&D API (we store URL, not the actual image)
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Monster type for filtering (beast, humanoid, etc.)
        [MaxLength(50)]
        public string MonsterType { get; set; } = "humanoid";

        // Flag to indicate if this monster has only physical attacks
        public bool HasOnlyPhysicalAttacks { get; set; } = true;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Additional stats that might be useful
        public int? AttackBonus { get; set; }
        
        [MaxLength(200)]
        public string? AttackName { get; set; } // e.g., "Scimitar", "Bite"
    }
}