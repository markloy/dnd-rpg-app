using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DndRpgApi.Models
{
    public class Character
    {
        [Key]
        public int Id { get; set; }

        // ===== BASIC CHARACTER INFO =====
        
        [Required(ErrorMessage = "Character name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Character name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Level must be between 1 and 20")]
        public int Level { get; set; } = 1;

        [Required(ErrorMessage = "User ID is required")]
        [StringLength(450)] // Standard Identity user ID length
        public string UserId { get; set; } = string.Empty;

        // ===== D&D CHARACTER DETAILS =====
        
        [Required(ErrorMessage = "Character class is required")]
        [StringLength(50, ErrorMessage = "Character class cannot exceed 50 characters")]
        public string CharacterClass { get; set; } = "Fighter";

        [Required(ErrorMessage = "Background is required")]
        [StringLength(50, ErrorMessage = "Background cannot exceed 50 characters")]
        public string Background { get; set; } = "Soldier";

        [Required(ErrorMessage = "Race is required")]
        [StringLength(50, ErrorMessage = "Race cannot exceed 50 characters")]
        public string Race { get; set; } = "Human";

        // ===== ABILITY SCORES =====
        
        [Range(1, 20, ErrorMessage = "Strength must be between 1 and 20")]
        public int Strength { get; set; } = 10;

        [Range(1, 20, ErrorMessage = "Dexterity must be between 1 and 20")]
        public int Dexterity { get; set; } = 10;

        [Range(1, 20, ErrorMessage = "Constitution must be between 1 and 20")]
        public int Constitution { get; set; } = 10;

        [Range(1, 20, ErrorMessage = "Intelligence must be between 1 and 20")]
        public int Intelligence { get; set; } = 10;

        [Range(1, 20, ErrorMessage = "Wisdom must be between 1 and 20")]
        public int Wisdom { get; set; } = 10;

        [Range(1, 20, ErrorMessage = "Charisma must be between 1 and 20")]
        public int Charisma { get; set; } = 10;

        // ===== HEALTH AND EXPERIENCE =====
        
        [Range(0, 999, ErrorMessage = "Health must be between 0 and 999")]
        public int Health { get; set; } = 100;

        [Range(1, 999, ErrorMessage = "Max health must be between 1 and 999")]
        public int MaxHealth { get; set; } = 100;

        [Range(0, 999999, ErrorMessage = "Experience must be between 0 and 999,999")]
        public int Experience { get; set; } = 0;

        [Range(0, 999999, ErrorMessage = "Experience to next level must be between 0 and 999,999")]
        public int ExperienceToNextLevel { get; set; } = 300;

        [Range(0, 999999, ErrorMessage = "Total experience must be between 0 and 999,999")]
        public int TotalExperience { get; set; } = 0;

        [Range(0, 999999, ErrorMessage = "Gold must be between 0 and 999,999")]
        public int Gold { get; set; } = 100;

        // ===== EQUIPMENT =====
        
        [StringLength(50, ErrorMessage = "Weapon name cannot exceed 50 characters")]
        public string WeaponName { get; set; } = "Longsword";

        [StringLength(20, ErrorMessage = "Weapon damage cannot exceed 20 characters")]
        [RegularExpression(@"^(\d+d\d+(\+\d+)?|0)$", ErrorMessage = "Weapon damage must be in format like '1d8+2'")]
        public string WeaponDamage { get; set; } = "1d8";

        [Range(-5, 20, ErrorMessage = "Weapon attack bonus must be between -5 and 20")]
        public int WeaponAttackBonus { get; set; } = 2;

        [StringLength(50, ErrorMessage = "Armor name cannot exceed 50 characters")]
        public string ArmorName { get; set; } = "Chainmail";

        [Range(6, 30, ErrorMessage = "Armor class must be between 6 and 30")]
        public int ArmorClass { get; set; } = 10;

        [StringLength(50, ErrorMessage = "Shield name cannot exceed 50 characters")]
        public string ShieldName { get; set; } = "";

        [Range(0, 10, ErrorMessage = "Shield AC bonus must be between 0 and 10")]
        public int ShieldArmorClassBonus { get; set; } = 0;

        // ===== TIMESTAMPS =====
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== NAVIGATION PROPERTIES =====
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // ===== COMPUTED PROPERTIES =====
        // These are calculated values that don't get stored in the database
        
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
        public int TotalArmorClass => ArmorClass + ShieldArmorClassBonus;

        [NotMapped]
        public double HealthPercentage => MaxHealth > 0 ? (double)Health / MaxHealth * 100 : 0;

        [NotMapped]
        public int ExperienceNeeded => Math.Max(0, ExperienceToNextLevel - Experience);

        [NotMapped]
        public string HealthStatus => HealthPercentage switch
        {
            >= 75 => "Healthy",
            >= 50 => "Wounded",
            >= 25 => "Bloodied",
            > 0 => "Near Death",
            _ => "Unconscious"
        };

        [NotMapped]
        public bool IsAlive => Health > 0;

        [NotMapped]
        public bool CanLevelUp => Experience >= ExperienceToNextLevel && Level < 20;

        // ===== HELPER METHODS =====
        
        /// <summary>
        /// Get the ability modifier for a given ability score
        /// </summary>
        public static int GetAbilityModifier(int abilityScore)
        {
            return (abilityScore - 10) / 2;
        }

        /// <summary>
        /// Get formatted ability modifier with + or - sign
        /// </summary>
        public string GetFormattedModifier(int abilityScore)
        {
            var modifier = GetAbilityModifier(abilityScore);
            return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
        }

        /// <summary>
        /// Get proficiency bonus based on character level
        /// </summary>
        [NotMapped]
        public int ProficiencyBonus => Level switch
        {
            >= 17 => 6,
            >= 13 => 5,
            >= 9 => 4,
            >= 5 => 3,
            _ => 2
        };

        /// <summary>
        /// Calculate spell attack bonus (for spellcasters)
        /// </summary>
        [NotMapped]
        public int SpellAttackBonus
        {
            get
            {
                var spellcastingModifier = CharacterClass.ToLower() switch
                {
                    "wizard" => IntelligenceModifier,
                    "cleric" or "druid" or "ranger" => WisdomModifier,
                    "bard" or "paladin" or "sorcerer" or "warlock" => CharismaModifier,
                    _ => 0
                };
                return ProficiencyBonus + spellcastingModifier;
            }
        }

        /// <summary>
        /// Calculate spell save DC (for spellcasters)
        /// </summary>
        [NotMapped]
        public int SpellSaveDC => 8 + SpellAttackBonus;

        /// <summary>
        /// Get character's initiative bonus
        /// </summary>
        [NotMapped]
        public int InitiativeBonus => DexterityModifier;

        /// <summary>
        /// Get character's passive perception
        /// </summary>
        [NotMapped]
        public int PassivePerception => 10 + WisdomModifier + (HasSkillProficiency("Perception") ? ProficiencyBonus : 0);

        /// <summary>
        /// Check if character has proficiency in a skill (simplified)
        /// </summary>
        public bool HasSkillProficiency(string skill)
        {
            // This is a simplified check - in a full implementation, 
            // you'd store skill proficiencies in the database
            return CharacterClass.ToLower() switch
            {
                "rogue" => skill.ToLower() is "stealth" or "sleight of hand" or "thieves' tools",
                "ranger" => skill.ToLower() is "survival" or "animal handling" or "perception",
                "cleric" => skill.ToLower() is "insight" or "religion",
                "wizard" => skill.ToLower() is "arcana" or "investigation",
                _ => false
            };
        }

        /// <summary>
        /// Get a summary string of the character
        /// </summary>
        public override string ToString()
        {
            return $"{Name}, Level {Level} {Race} {CharacterClass} ({Health}/{MaxHealth} HP)";
        }
    }
}