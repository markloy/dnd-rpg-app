using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DndRpgApi.Models
{
    public class Monster
    {
        [Key]
        public int Id { get; set; }

        // ===== BASIC MONSTER INFO =====
        
        [Required(ErrorMessage = "Monster name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Monster name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Monster type cannot exceed 50 characters")]
        [Required(ErrorMessage = "Monster type is required")]
        public string Type { get; set; } = string.Empty; // Dragon, Humanoid, Beast, etc.

        [StringLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string Size { get; set; } = "Medium"; // Tiny, Small, Medium, Large, Huge, Gargantuan

        [StringLength(50, ErrorMessage = "Alignment cannot exceed 50 characters")]
        public string Alignment { get; set; } = "Neutral";

        [Range(0, 30, ErrorMessage = "Challenge rating must be between 0 and 30")]
        [Column(TypeName = "decimal(4,3)")]
        public decimal ChallengeRating { get; set; } = 1;

        [Range(0, 999999, ErrorMessage = "Experience value must be between 0 and 999,999")]
        public int ExperienceValue { get; set; } = 200;

        // ===== COMBAT STATS =====
        
        [Range(1, 999, ErrorMessage = "Hit points must be between 1 and 999")]
        public int HitPoints { get; set; } = 10;

        [StringLength(20, ErrorMessage = "Hit dice cannot exceed 20 characters")]
        [RegularExpression(@"^(\d+d\d+(\+\d+)?|0)$", ErrorMessage = "Hit dice must be in format like '2d8+2'")]
        public string HitDice { get; set; } = "2d8";

        [Range(6, 30, ErrorMessage = "Armor class must be between 6 and 30")]
        public int ArmorClass { get; set; } = 12;

        [StringLength(50, ErrorMessage = "Armor description cannot exceed 50 characters")]
        public string ArmorDescription { get; set; } = "Natural Armor";

        // ===== MOVEMENT =====
        
        [Range(0, 200, ErrorMessage = "Speed must be between 0 and 200 feet")]
        public int Speed { get; set; } = 30;

        [Range(0, 200, ErrorMessage = "Fly speed must be between 0 and 200 feet")]
        public int? FlySpeed { get; set; }

        [Range(0, 200, ErrorMessage = "Swim speed must be between 0 and 200 feet")]
        public int? SwimSpeed { get; set; }

        [Range(0, 200, ErrorMessage = "Climb speed must be between 0 and 200 feet")]
        public int? ClimbSpeed { get; set; }

        // ===== ABILITY SCORES =====
        
        [Range(1, 30, ErrorMessage = "Strength must be between 1 and 30")]
        public int Strength { get; set; } = 10;

        [Range(1, 30, ErrorMessage = "Dexterity must be between 1 and 30")]
        public int Dexterity { get; set; } = 10;

        [Range(1, 30, ErrorMessage = "Constitution must be between 1 and 30")]
        public int Constitution { get; set; } = 10;

        [Range(1, 30, ErrorMessage = "Intelligence must be between 1 and 30")]
        public int Intelligence { get; set; } = 10;

        [Range(1, 30, ErrorMessage = "Wisdom must be between 1 and 30")]
        public int Wisdom { get; set; } = 10;

        [Range(1, 30, ErrorMessage = "Charisma must be between 1 and 30")]
        public int Charisma { get; set; } = 10;

        // ===== SAVES AND SKILLS =====
        
        [StringLength(200, ErrorMessage = "Saving throws cannot exceed 200 characters")]
        public string? SavingThrows { get; set; } // e.g., "Dex +5, Con +3"

        [StringLength(300, ErrorMessage = "Skills cannot exceed 300 characters")]
        public string? Skills { get; set; } // e.g., "Perception +4, Stealth +6"

        [StringLength(300, ErrorMessage = "Damage resistances cannot exceed 300 characters")]
        public string? DamageResistances { get; set; }

        [StringLength(300, ErrorMessage = "Damage immunities cannot exceed 300 characters")]
        public string? DamageImmunities { get; set; }

        [StringLength(300, ErrorMessage = "Condition immunities cannot exceed 300 characters")]
        public string? ConditionImmunities { get; set; }

        [StringLength(200, ErrorMessage = "Senses cannot exceed 200 characters")]
        public string? Senses { get; set; } // e.g., "Darkvision 60 ft., passive Perception 14"

        [StringLength(200, ErrorMessage = "Languages cannot exceed 200 characters")]
        public string? Languages { get; set; }

        // ===== SPECIAL ABILITIES =====
        
        [StringLength(2000, ErrorMessage = "Special abilities cannot exceed 2000 characters")]
        public string? SpecialAbilities { get; set; } // JSON or formatted text of abilities

        // ===== ACTIONS =====
        
        [Required(ErrorMessage = "Actions are required")]
        [StringLength(2000, ErrorMessage = "Actions cannot exceed 2000 characters")]
        public string Actions { get; set; } = string.Empty; // JSON or formatted text of actions

        [StringLength(1000, ErrorMessage = "Legendary actions cannot exceed 1000 characters")]
        public string? LegendaryActions { get; set; }

        [StringLength(1000, ErrorMessage = "Lair actions cannot exceed 1000 characters")]
        public string? LairActions { get; set; }

        // ===== METADATA =====
        
        [StringLength(50, ErrorMessage = "Source cannot exceed 50 characters")]
        public string Source { get; set; } = "Monster Manual"; // Which D&D book

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        // ===== D&D 5e API INTEGRATION =====
        
        [StringLength(100, ErrorMessage = "API ID cannot exceed 100 characters")]
        public string? ApiId { get; set; } // ID from D&D 5e API (e.g., "goblin", "ancient-red-dragon")

        [StringLength(200, ErrorMessage = "API URL cannot exceed 200 characters")]
        public string? ApiUrl { get; set; } // Full URL to D&D 5e API endpoint

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== COMPUTED PROPERTIES =====
        
        [NotMapped]
        public int StrengthModifier => (Strength - 10) / 2;

        [NotMapped]
        public int DexterityModifier => (Dexterity - 10) / 2;

        [NotMapped]
        public int ConstitutionModifier => (Constitution - 10) / 2;

        [NotMapped]
        public int IntelligenceModifier => (Intelligence - 10) / 2;

        [NotMapped]
        public int WisdomModifier => (Wisdom - 10) / 2;

        [NotMapped]
        public int CharismaModifier => (Charisma - 10) / 2;

        [NotMapped]
        public int ProficiencyBonus => ChallengeRating switch
        {
            <= 4 => 2,
            <= 8 => 3,
            <= 12 => 4,
            <= 16 => 5,
            <= 20 => 6,
            <= 24 => 7,
            <= 28 => 8,
            _ => 9
        };

        [NotMapped]
        public string ChallengeRatingDisplay => ChallengeRating < 1 
            ? $"1/{(int)(1 / ChallengeRating)}" 
            : ChallengeRating.ToString("0.###");

        [NotMapped]
        public string SpeedDisplay
        {
            get
            {
                var speeds = new List<string> { $"{Speed} ft." };
                
                if (FlySpeed.HasValue) speeds.Add($"fly {FlySpeed.Value} ft.");
                if (SwimSpeed.HasValue) speeds.Add($"swim {SwimSpeed.Value} ft.");
                if (ClimbSpeed.HasValue) speeds.Add($"climb {ClimbSpeed.Value} ft.");
                
                return string.Join(", ", speeds);
            }
        }

        // ===== HELPER METHODS =====
        
        public int GetSavingThrowBonus(string ability)
        {
            if (string.IsNullOrEmpty(SavingThrows) || !SavingThrows.Contains(ability, StringComparison.OrdinalIgnoreCase))
            {
                // No proficiency, just ability modifier
                return ability.ToLower() switch
                {
                    "str" or "strength" => StrengthModifier,
                    "dex" or "dexterity" => DexterityModifier,
                    "con" or "constitution" => ConstitutionModifier,
                    "int" or "intelligence" => IntelligenceModifier,
                    "wis" or "wisdom" => WisdomModifier,
                    "cha" or "charisma" => CharismaModifier,
                    _ => 0
                };
            }
            
            // Parse the specific bonus from SavingThrows string
            // This is a simplified implementation - in a real system you'd parse the string properly
            var abilityMod = ability.ToLower() switch
            {
                "str" or "strength" => StrengthModifier,
                "dex" or "dexterity" => DexterityModifier,
                "con" or "constitution" => ConstitutionModifier,
                "int" or "intelligence" => IntelligenceModifier,
                "wis" or "wisdom" => WisdomModifier,
                "cha" or "charisma" => CharismaModifier,
                _ => 0
            };
            
            return abilityMod + ProficiencyBonus;
        }
    }
}