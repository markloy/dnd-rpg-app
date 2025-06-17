using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DndRpgApi.Models
{
    // Extends IdentityUser to add custom properties
    // IdentityUser provides: Id, UserName, Email, PasswordHash, etc.
    public class ApplicationUser : IdentityUser
    {
        // Custom properties for our D&D app
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        // Navigation property - one user can have multiple characters
        public virtual ICollection<Character> Characters { get; set; } = new List<Character>();

        // Display name property for UI
        public string DisplayName => 
            !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
                ? $"{FirstName} {LastName}" 
                : UserName ?? Email ?? "Unknown User";
    }
}