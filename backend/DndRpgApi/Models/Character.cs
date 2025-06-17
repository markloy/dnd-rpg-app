using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DndRpgApi.Models
{
    public class Character
    {
        [Key]
        public int Id { get; set; }

        // ===== BASIC CHARACTER INFO =====
        
        [Required(ErrorMessage = "Character name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Character name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Character name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Level must be between 1 and 20")]
        public int Level { get; set; } = 1;

        [Range(1, 999, ErrorMessage = "Health must be between 1 and 999")]
        public int Health { get; set; } = 100;

        [Range(1, 999, ErrorMessage = "Maximum health must be between 1 and 999")]
        public int MaxHealth { get; set; } = 100;

        [Range(0, 999999, ErrorMessage = "Experience must be between 0 and 999,999")]
        public int Experience { get; set; } = 0;

        [Range(1, 999999, ErrorMessage = "Experience to next level must be between 1 and 999,999")]
        public int ExperienceToNextLevel { get; set; } = 300;

        [Range(0, 999999, ErrorMessage = "Total experience must be between 0 and 999,999")]
        public int TotalExperience { get; set; } = 0;

        [Range(0, 999999, ErrorMessage = "Gold must be between 0 and 999,999")]
        public int Gold { get; set; } = 100;

        // ===== USER RELATIONSHIP =====
        
        [Required(ErrorMessage = "User ID is required")]
        [StringLength(450, ErrorMessage = "User ID cannot exceed 450 characters")]
        public string UserId { get; set; } = string.Empty;

        public virtual ApplicationUser? User { get; set; }

        // ===== ABILITY SCORES (D&D Standard: 1-20) =====
        
        [Range(1, 20, ErrorMessage = "Strength must be between 1 and 20")]
        [Display(Name = "Strength", Description = "Physical power and muscle")]
        public int Strength { get; set; } = 16;

        [Range(1, 20, ErrorMessage = "Dexterity must be between 1 and 20")]
        [Display(Name = "Dexterity", Description = "Agility, reflexes, and balance")]
        public int Dexterity { get; set; } = 14;

        [Range(1, 20, ErrorMessage = "Constitution must be between 1 and 20")]
        [Display(Name = "Constitution", Description = "Health, stamina, and vitality")]
        public int Constitution { get; set; } = 15;

        [Range(1, 20, ErrorMessage = "Intelligence must be between 1 and 20")]
        [Display(Name = "Intelligence", Description = "Reasoning ability and memory")]
        public int Intelligence { get; set; } = 12;

        [Range(1, 20, ErrorMessage = "Wisdom must be between 1 and 20")]
        [Display(Name = "Wisdom", Description = "Awareness, intuition, and insight")]
        public int Wisdom { get; set; } = 13;

        [Range(1, 20, ErrorMessage = "Charisma must be between 1 and 20")]
        [Display(Name = "Charisma", Description = "Force of personality and leadership")]
        public int Charisma { get; set; } = 10;

        // ===== EQUIPMENT =====
        
        [StringLength(50, ErrorMessage = "Weapon name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.+]+$", ErrorMessage = "Weapon name contains invalid characters")]
        public string WeaponName { get; set; } = "Longsword";
        
        [StringLength(20, ErrorMessage = "Weapon damage cannot exceed 20 characters")]
        [RegularExpression(@"^(\d+d\d+(\+\d+)?|0)$", ErrorMessage = "Weapon damage must be in format like '1d8+3' or '2d6'")]
        public string WeaponDamage { get; set; } = "1d8";
        
        [Range(-10, 20, ErrorMessage = "Weapon attack bonus must be between -10 and +20")]
        public int WeaponAttackBonus { get; set; } = 3;
        
        [StringLength(50, ErrorMessage = "Armor name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Armor name contains invalid characters")]
        public string ArmorName { get; set; } = "Chainmail";
        
        [Range(10, 30, ErrorMessage = "Armor Class must be between 10 and 30")]
        public int ArmorClass { get; set; } = 16;
        
        [StringLength(50, ErrorMessage = "Shield name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]*$", ErrorMessage = "Shield name contains invalid characters")]
        public string ShieldName { get; set; } = "Shield";
        
        [Range(0, 10, ErrorMessage = "Shield AC bonus must be between 0 and 10")]
        public int ShieldArmorClassBonus { get; set; } = 2;

        // ===== AUDIT FIELDS =====
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== COMPUTED PROPERTIES =====
        
        /// <summary>
        /// Calculated ability modifier based on D&D 5e rules
        /// </summary>
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

        /// <summary>
        /// Total Armor Class including shield bonus
        /// </summary>
        [NotMapped]
        public int TotalArmorClass => ArmorClass + ShieldArmorClassBonus;

        /// <summary>
        /// Character's current health percentage
        /// </summary>
        [NotMapped]
        public decimal HealthPercentage => MaxHealth > 0 ? (decimal)Health / MaxHealth * 100 : 0;

        /// <summary>
        /// Experience needed until next level
        /// </summary>
        [NotMapped]
        public int ExperienceNeeded => Math.Max(0, ExperienceToNextLevel - Experience);
    }

    // ===== CUSTOM VALIDATION ATTRIBUTES =====

    /// <summary>
    /// Validates that Health never exceeds MaxHealth
    /// </summary>
    public class HealthNotExceedingMaxHealthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var character = (Character)validationContext.ObjectInstance;
            
            if (character.Health > character.MaxHealth)
            {
                // Handle nullable MemberName safely
                var memberNames = validationContext.MemberName != null 
                    ? new[] { validationContext.MemberName } 
                    : Array.Empty<string>();
                    
                return new ValidationResult(
                    "Current health cannot exceed maximum health",
                    memberNames);
            }
            
            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validates D&D dice notation (e.g., "1d8+3", "2d6")
    /// </summary>
    public class DiceNotationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string diceString || string.IsNullOrEmpty(diceString))
                return ValidationResult.Success; // Let Required attribute handle null/empty

            var pattern = @"^(\d+d\d+(\+\d+)?|0)$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(diceString, pattern))
            {
                // Handle nullable MemberName safely
                var memberNames = validationContext.MemberName != null 
                    ? new[] { validationContext.MemberName } 
                    : Array.Empty<string>();
                    
                return new ValidationResult(
                    "Dice notation must be in format like '1d8', '1d8+3', '2d6+1', or '0'",
                    memberNames);
            }
            
            return ValidationResult.Success;
        }
    }
}