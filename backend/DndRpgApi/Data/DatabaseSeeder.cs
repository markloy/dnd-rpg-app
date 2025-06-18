using DndRpgApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DndRpgApi.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting database seeding...");
                
                // Seed monsters first (they don't depend on other entities)
                await SeedMonstersAsync(context, logger);
                
                // Seed sample characters
                await SeedCharactersAsync(context, logger);
                
                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }
        
        private static async Task SeedMonstersAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.Monsters.AnyAsync())
            {
                logger.LogInformation("Monsters already seeded, skipping monster seeding");
                return;
            }
            
            logger.LogInformation("Seeding monsters...");
            
            var monsters = new List<Monster>
            {
                // ===== LOW CHALLENGE RATING MONSTERS =====
                
                new Monster
                {
                    Name = "Goblin",
                    Type = "Humanoid",
                    Size = "Small",
                    Alignment = "Neutral Evil",
                    ChallengeRating = 0.25m,
                    ExperienceValue = 50,
                    HitPoints = 7,
                    HitDice = "2d6",
                    ArmorClass = 15,
                    ArmorDescription = "Leather Armor, Shield",
                    Speed = 30,
                    Strength = 8,
                    Dexterity = 14,
                    Constitution = 10,
                    Intelligence = 10,
                    Wisdom = 8,
                    Charisma = 8,
                    Skills = "Stealth +6",
                    Senses = "Darkvision 60 ft., passive Perception 9",
                    Languages = "Common, Goblin",
                    Actions = "Scimitar: Melee Weapon Attack: +4 to hit, reach 5 ft., one target. Hit: 5 (1d6 + 2) slashing damage. Shortbow: Ranged Weapon Attack: +4 to hit, range 80/320 ft., one target. Hit: 5 (1d6 + 2) piercing damage.",
                    Source = "Monster Manual",
                    Description = "Small, evil humanoids that live in dark places underground."
                },
                
                new Monster
                {
                    Name = "Wolf",
                    Type = "Beast",
                    Size = "Medium",
                    Alignment = "Unaligned",
                    ChallengeRating = 0.25m,
                    ExperienceValue = 50,
                    HitPoints = 11,
                    HitDice = "2d8+2",
                    ArmorClass = 13,
                    ArmorDescription = "Natural Armor",
                    Speed = 40,
                    Strength = 12,
                    Dexterity = 15,
                    Constitution = 12,
                    Intelligence = 3,
                    Wisdom = 12,
                    Charisma = 6,
                    Skills = "Perception +3, Stealth +4",
                    Senses = "passive Perception 13",
                    Languages = "",
                    SpecialAbilities = "Keen Hearing and Smell: The wolf has advantage on Wisdom (Perception) checks that rely on hearing or smell. Pack Tactics: The wolf has advantage on an attack roll against a creature if at least one of the wolf's allies is within 5 feet of the creature and the ally isn't incapacitated.",
                    Actions = "Bite: Melee Weapon Attack: +4 to hit, reach 5 ft., one target. Hit: 7 (2d4 + 2) piercing damage. If the target is a creature, it must succeed on a DC 11 Strength saving throw or be knocked prone.",
                    Source = "Monster Manual",
                    Description = "A fierce predator that hunts in packs."
                },
                
                new Monster
                {
                    Name = "Orc",
                    Type = "Humanoid",
                    Size = "Medium",
                    Alignment = "Chaotic Evil",
                    ChallengeRating = 0.5m,
                    ExperienceValue = 100,
                    HitPoints = 15,
                    HitDice = "2d8+6",
                    ArmorClass = 13,
                    ArmorDescription = "Hide Armor",
                    Speed = 30,
                    Strength = 16,
                    Dexterity = 12,
                    Constitution = 16,
                    Intelligence = 7,
                    Wisdom = 11,
                    Charisma = 10,
                    Skills = "Intimidation +2",
                    Senses = "Darkvision 60 ft., passive Perception 10",
                    Languages = "Common, Orc",
                    SpecialAbilities = "Aggressive: As a bonus action, the orc can move up to its speed toward a hostile creature that it can see.",
                    Actions = "Greataxe: Melee Weapon Attack: +5 to hit, reach 5 ft., one target. Hit: 9 (1d12 + 3) slashing damage. Javelin: Melee or Ranged Weapon Attack: +5 to hit, reach 5 ft. or range 30/120 ft., one target. Hit: 6 (1d6 + 3) piercing damage.",
                    Source = "Monster Manual",
                    Description = "Savage raiders and pillagers with a lust for battle."
                },
                
                // ===== MEDIUM CHALLENGE RATING MONSTERS =====
                
                new Monster
                {
                    Name = "Owlbear",
                    Type = "Monstrosity",
                    Size = "Large",
                    Alignment = "Unaligned",
                    ChallengeRating = 3,
                    ExperienceValue = 700,
                    HitPoints = 59,
                    HitDice = "7d10+21",
                    ArmorClass = 13,
                    ArmorDescription = "Natural Armor",
                    Speed = 40,
                    Strength = 20,
                    Dexterity = 12,
                    Constitution = 17,
                    Intelligence = 3,
                    Wisdom = 12,
                    Charisma = 7,
                    Skills = "Perception +3",
                    Senses = "Darkvision 60 ft., passive Perception 13",
                    Languages = "",
                    SpecialAbilities = "Keen Sight and Smell: The owlbear has advantage on Wisdom (Perception) checks that rely on sight or smell.",
                    Actions = "Multiattack: The owlbear makes two attacks: one with its beak and one with its claws. Beak: Melee Weapon Attack: +7 to hit, reach 5 ft., one creature. Hit: 10 (1d10 + 5) piercing damage. Claws: Melee Weapon Attack: +7 to hit, reach 5 ft., one target. Hit: 14 (2d8 + 5) slashing damage.",
                    Source = "Monster Manual",
                    Description = "A cross between a giant owl and a bear, this creature is both curious and dangerous."
                },
                
                new Monster
                {
                    Name = "Troll",
                    Type = "Giant",
                    Size = "Large",
                    Alignment = "Chaotic Evil",
                    ChallengeRating = 5,
                    ExperienceValue = 1800,
                    HitPoints = 84,
                    HitDice = "8d10+40",
                    ArmorClass = 15,
                    ArmorDescription = "Natural Armor",
                    Speed = 30,
                    Strength = 18,
                    Dexterity = 13,
                    Constitution = 20,
                    Intelligence = 7,
                    Wisdom = 9,
                    Charisma = 7,
                    Skills = "Perception +2",
                    Senses = "Darkvision 60 ft., passive Perception 12",
                    Languages = "Giant",
                    SpecialAbilities = "Keen Smell: The troll has advantage on Wisdom (Perception) checks that rely on smell. Regeneration: The troll regains 10 hit points at the start of its turn. If the troll takes acid or fire damage, this trait doesn't function at the start of the troll's next turn. The troll dies only if it starts its turn with 0 hit points and doesn't regenerate.",
                    Actions = "Multiattack: The troll makes three attacks: one with its bite and two with its claws. Bite: Melee Weapon Attack: +7 to hit, reach 5 ft., one target. Hit: 7 (1d6 + 4) piercing damage. Claw: Melee Weapon Attack: +7 to hit, reach 5 ft., one target. Hit: 11 (2d6 + 4) slashing damage.",
                    Source = "Monster Manual",
                    Description = "A fearsome giant with incredible regenerative abilities."
                },
                
                // ===== HIGH CHALLENGE RATING MONSTERS =====
                
                new Monster
                {
                    Name = "Young Red Dragon",
                    Type = "Dragon",
                    Size = "Large",
                    Alignment = "Chaotic Evil",
                    ChallengeRating = 10,
                    ExperienceValue = 5900,
                    HitPoints = 178,
                    HitDice = "17d12+85",
                    ArmorClass = 18,
                    ArmorDescription = "Natural Armor",
                    Speed = 40,
                    FlySpeed = 80,
                    ClimbSpeed = 40,
                    Strength = 23,
                    Dexterity = 10,
                    Constitution = 21,
                    Intelligence = 14,
                    Wisdom = 11,
                    Charisma = 19,
                    SavingThrows = "Dex +4, Con +9, Wis +4, Cha +8",
                    Skills = "Perception +8, Stealth +4",
                    DamageImmunities = "Fire",
                    Senses = "Blindsight 30 ft., Darkvision 120 ft., passive Perception 18",
                    Languages = "Common, Draconic",
                    Actions = "Multiattack: The dragon makes three attacks: one with its bite and two with its claws. Bite: Melee Weapon Attack: +10 to hit, reach 10 ft., one target. Hit: 17 (2d10 + 6) piercing damage plus 3 (1d6) fire damage. Claw: Melee Weapon Attack: +10 to hit, reach 5 ft., one target. Hit: 13 (2d6 + 6) slashing damage. Fire Breath (Recharge 5-6): The dragon exhales fire in a 30-foot cone. Each creature in that area must make a DC 17 Dexterity saving throw, taking 56 (16d6) fire damage on a failed save, or half as much damage on a successful one.",
                    Source = "Monster Manual",
                    Description = "A proud and powerful young dragon with burning ambition and a fiery temper."
                },
                
                new Monster
                {
                    Name = "Beholder",
                    Type = "Aberration",
                    Size = "Large",
                    Alignment = "Lawful Evil",
                    ChallengeRating = 13,
                    ExperienceValue = 10000,
                    HitPoints = 180,
                    HitDice = "19d12+76",
                    ArmorClass = 18,
                    ArmorDescription = "Natural Armor",
                    Speed = 0,
                    FlySpeed = 20,
                    Strength = 10,
                    Dexterity = 14,
                    Constitution = 18,
                    Intelligence = 17,
                    Wisdom = 15,
                    Charisma = 17,
                    SavingThrows = "Int +8, Wis +7, Cha +8",
                    Skills = "Perception +12",
                    ConditionImmunities = "Prone",
                    Senses = "Darkvision 120 ft., passive Perception 22",
                    Languages = "Deep Speech, Undercommon",
                    SpecialAbilities = "Antimagic Cone: The beholder's central eye creates an area of antimagic, as in the antimagic field spell, in a 150-foot cone. At the start of each of its turns, the beholder decides which way the cone faces and whether the cone is active. The area works against the beholder's own eye rays.",
                    Actions = "Bite: Melee Weapon Attack: +5 to hit, reach 5 ft., one target. Hit: 14 (4d6) piercing damage. Eye Rays: The beholder shoots three of the following magical eye rays at random (reroll duplicates), choosing one to three targets it can see within 120 feet of it: 1. Charm Ray, 2. Paralyzing Ray, 3. Fear Ray, 4. Slowing Ray, 5. Enervation Ray, 6. Telekinetic Ray, 7. Sleep Ray, 8. Petrification Ray, 9. Disintegration Ray, 10. Death Ray.",
                    LegendaryActions = "The beholder can take 3 legendary actions, choosing from the options below. Only one legendary action option can be used at a time and only at the end of another creature's turn. Eye Ray: The beholder uses one random eye ray.",
                    Source = "Monster Manual",
                    Description = "A floating orb of flesh with a large eye and many eyestalks, each capable of deadly magic."
                }
            };
            
            await context.Monsters.AddRangeAsync(monsters);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded {Count} monsters", monsters.Count);
        }
        
        private static async Task SeedCharactersAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.Characters.AnyAsync())
            {
                logger.LogInformation("Characters already seeded, skipping character seeding");
                return;
            }
            
            logger.LogInformation("Seeding sample characters...");
            
            var characters = new List<Character>
            {
                new Character
                {
                    Name = "Aragorn",
                    Level = 8,
                    Health = 75,
                    MaxHealth = 75,
                    Experience = 25000,
                    ExperienceToNextLevel = 48000,
                    TotalExperience = 25000,
                    Gold = 850,
                    UserId = "sample-user-1",
                    CharacterClass = "Ranger",
                    Background = "Outlander",
                    Race = "Human",
                    Strength = 16,
                    Dexterity = 18,
                    Constitution = 15,
                    Intelligence = 14,
                    Wisdom = 17,
                    Charisma = 15,
                    WeaponName = "Longsword",
                    WeaponDamage = "1d8+3",
                    WeaponAttackBonus = 7,
                    ArmorName = "Studded Leather",
                    ArmorClass = 15,
                    ShieldName = "",
                    ShieldArmorClassBonus = 0
                },
                
                new Character
                {
                    Name = "Gandalf",
                    Level = 12,
                    Health = 95,
                    MaxHealth = 95,
                    Experience = 15000,
                    ExperienceToNextLevel = 120000,
                    TotalExperience = 120000,
                    Gold = 1200,
                    UserId = "sample-user-2",
                    CharacterClass = "Wizard",
                    Background = "Sage",
                    Race = "Human",
                    Strength = 10,
                    Dexterity = 13,
                    Constitution = 16,
                    Intelligence = 20,
                    Wisdom = 18,
                    Charisma = 16,
                    WeaponName = "Staff",
                    WeaponDamage = "1d6",
                    WeaponAttackBonus = 2,
                    ArmorName = "Robes",
                    ArmorClass = 12,
                    ShieldName = "",
                    ShieldArmorClassBonus = 0
                },
                
                new Character
                {
                    Name = "Legolas",
                    Level = 6,
                    Health = 55,
                    MaxHealth = 55,
                    Experience = 8000,
                    ExperienceToNextLevel = 23000,
                    TotalExperience = 15000,
                    Gold = 450,
                    UserId = "sample-user-3",
                    CharacterClass = "Fighter",
                    Background = "Noble",
                    Race = "Elf",
                    Strength = 14,
                    Dexterity = 20,
                    Constitution = 14,
                    Intelligence = 15,
                    Wisdom = 16,
                    Charisma = 13,
                    WeaponName = "Elven Bow",
                    WeaponDamage = "1d8+5",
                    WeaponAttackBonus = 8,
                    ArmorName = "Elven Chain",
                    ArmorClass = 16,
                    ShieldName = "",
                    ShieldArmorClassBonus = 0
                },
                
                new Character
                {
                    Name = "Gimli",
                    Level = 7,
                    Health = 68,
                    MaxHealth = 68,
                    Experience = 12000,
                    ExperienceToNextLevel = 34000,
                    TotalExperience = 22000,
                    Gold = 920,
                    UserId = "sample-user-4",
                    CharacterClass = "Fighter",
                    Background = "Guild Artisan",
                    Race = "Dwarf",
                    Strength = 18,
                    Dexterity = 12,
                    Constitution = 18,
                    Intelligence = 11,
                    Wisdom = 14,
                    Charisma = 10,
                    WeaponName = "Dwarven Axe",
                    WeaponDamage = "1d12+4",
                    WeaponAttackBonus = 7,
                    ArmorName = "Plate Armor",
                    ArmorClass = 18,
                    ShieldName = "Shield",
                    ShieldArmorClassBonus = 2
                }
            };
            
            await context.Characters.AddRangeAsync(characters);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded {Count} sample characters", characters.Count);
        }
    }
}