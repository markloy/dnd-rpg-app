using DndRpgApi.Data;
using DndRpgApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DndRpgApi.Services
{
    // ===== BASE REPOSITORY IMPLEMENTATION =====
    
    /// <summary>
    /// Generic repository implementation with common CRUD operations
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;
        
        public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        // ===== QUERY OPERATIONS =====
        
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
            return await _dbSet.ToListAsync();
        }
        
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Getting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(id);
        }
        
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogDebug("Finding {EntityType} entities with predicate", typeof(T).Name);
            return await _dbSet.Where(predicate).ToListAsync();
        }
        
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogDebug("Getting first {EntityType} entity matching predicate", typeof(T).Name);
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }
        
        public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            _logger.LogDebug("Getting paged {EntityType} results: page {Page}, size {PageSize}", 
                typeof(T).Name, page, pageSize);
            
            var query = predicate == null ? _dbSet : _dbSet.Where(predicate);
            
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        
        // ===== MODIFICATION OPERATIONS =====
        
        public virtual async Task<T> AddAsync(T entity)
        {
            _logger.LogDebug("Adding new {EntityType} entity", typeof(T).Name);
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }
        
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            _logger.LogDebug("Adding {Count} {EntityType} entities", entities.Count(), typeof(T).Name);
            await _dbSet.AddRangeAsync(entities);
            await SaveChangesAsync();
        }
        
        public virtual async Task<T> UpdateAsync(T entity)
        {
            _logger.LogDebug("Updating {EntityType} entity", typeof(T).Name);
            _dbSet.Update(entity);
            await SaveChangesAsync();
            return entity;
        }
        
        public virtual async Task<bool> DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("{EntityType} entity with ID {Id} not found for deletion", typeof(T).Name, id);
                return false;
            }
            
            _dbSet.Remove(entity);
            await SaveChangesAsync();
            return true;
        }
        
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            _logger.LogDebug("Deleting {EntityType} entity", typeof(T).Name);
            _dbSet.Remove(entity);
            await SaveChangesAsync();
            return true;
        }
        
        public virtual async Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogDebug("Deleting {EntityType} entities matching predicate", typeof(T).Name);
            var entities = await _dbSet.Where(predicate).ToListAsync();
            _dbSet.RemoveRange(entities);
            await SaveChangesAsync();
            return entities.Count;
        }
        
        // ===== TRANSACTION OPERATIONS =====
        
        public virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error for {EntityType}", typeof(T).Name);
                throw;
            }
        }
        
        public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    // ===== CHARACTER REPOSITORY IMPLEMENTATION =====
    
    /// <summary>
    /// Character-specific repository with D&D game logic
    /// </summary>
    public class CharacterRepository : Repository<Character>, ICharacterRepository
    {
        public CharacterRepository(ApplicationDbContext context, ILogger<CharacterRepository> logger) 
            : base(context, logger) { }
        
        // ===== CHARACTER-SPECIFIC QUERIES =====
        
        public async Task<IEnumerable<Character>> GetByUserIdAsync(string userId)
        {
            _logger.LogDebug("Getting characters for user {UserId}", userId);
            return await _dbSet
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Character>> GetByLevelRangeAsync(int minLevel, int maxLevel)
        {
            _logger.LogDebug("Getting characters in level range {MinLevel}-{MaxLevel}", minLevel, maxLevel);
            return await _dbSet
                .Where(c => c.Level >= minLevel && c.Level <= maxLevel)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Character>> GetByClassAsync(string characterClass)
        {
            _logger.LogDebug("Getting characters of class {Class}", characterClass);
            return await _dbSet
                .Where(c => c.CharacterClass == characterClass)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Character>> SearchByNameAsync(string nameSearch)
        {
            _logger.LogDebug("Searching characters by name: {SearchTerm}", nameSearch);
            return await _dbSet
                .Where(c => c.Name.Contains(nameSearch))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        
        public async Task<Character?> GetWithEquipmentAsync(int characterId)
        {
            _logger.LogDebug("Getting character {CharacterId} with equipment details", characterId);
            
            // For now, just get the character (equipment is embedded in the model)
            // In a more complex system, you'd include related entities here
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Id == characterId);
        }
        
        public async Task<IEnumerable<Character>> GetReadyToLevelUpAsync()
        {
            _logger.LogDebug("Getting characters ready to level up");
            return await _dbSet
                .Where(c => c.Experience >= c.ExperienceToNextLevel)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        
        // ===== CHARACTER-SPECIFIC OPERATIONS =====
        
        public async Task<Character> AddExperienceAsync(int characterId, int experience)
        {
            _logger.LogInformation("Adding {Experience} experience to character {CharacterId}", experience, characterId);
            
            var character = await GetByIdAsync(characterId);
            if (character == null)
                throw new ArgumentException($"Character with ID {characterId} not found");
            
            character.Experience += experience;
            character.TotalExperience += experience;
            character.UpdatedAt = DateTime.UtcNow;
            
            // Check for level up
            while (character.Experience >= character.ExperienceToNextLevel && character.Level < 20)
            {
                LevelUpCharacterLogic(character);
            }
            
            await SaveChangesAsync();
            return character;
        }
        
        public async Task<Character> HealCharacterAsync(int characterId, int healAmount)
        {
            _logger.LogInformation("Healing character {CharacterId} for {HealAmount} HP", characterId, healAmount);
            
            var character = await GetByIdAsync(characterId);
            if (character == null)
                throw new ArgumentException($"Character with ID {characterId} not found");
            
            character.Health = Math.Min(character.Health + healAmount, character.MaxHealth);
            character.UpdatedAt = DateTime.UtcNow;
            
            await SaveChangesAsync();
            return character;
        }
        
        public async Task<Character> DamageCharacterAsync(int characterId, int damage)
        {
            _logger.LogInformation("Dealing {Damage} damage to character {CharacterId}", damage, characterId);
            
            var character = await GetByIdAsync(characterId);
            if (character == null)
                throw new ArgumentException($"Character with ID {characterId} not found");
            
            character.Health = Math.Max(character.Health - damage, 0);
            character.UpdatedAt = DateTime.UtcNow;
            
            if (character.Health == 0)
            {
                _logger.LogWarning("Character {CharacterName} has been reduced to 0 HP!", character.Name);
            }
            
            await SaveChangesAsync();
            return character;
        }
        
        public async Task<Character> LevelUpAsync(int characterId)
        {
            _logger.LogInformation("Leveling up character {CharacterId}", characterId);
            
            var character = await GetByIdAsync(characterId);
            if (character == null)
                throw new ArgumentException($"Character with ID {characterId} not found");
            
            if (character.Experience < character.ExperienceToNextLevel)
                throw new InvalidOperationException("Character does not have enough experience to level up");
            
            if (character.Level >= 20)
                throw new InvalidOperationException("Character is already at maximum level");
            
            LevelUpCharacterLogic(character);
            await SaveChangesAsync();
            
            return character;
        }
        
        public async Task<bool> IsNameAvailableAsync(string name, int? excludeCharacterId = null)
        {
            var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());
            
            if (excludeCharacterId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCharacterId.Value);
            }
            
            return !await query.AnyAsync();
        }
        
        // ===== PRIVATE HELPER METHODS =====
        
        private void LevelUpCharacterLogic(Character character)
        {
            _logger.LogInformation("Processing level up for {CharacterName} from level {OldLevel} to {NewLevel}", 
                character.Name, character.Level, character.Level + 1);
            
            // Calculate remaining experience after level up
            character.Experience -= character.ExperienceToNextLevel;
            character.Level++;
            
            // Calculate new experience requirement (simplified D&D 5e progression)
            character.ExperienceToNextLevel = CalculateExperienceForNextLevel(character.Level);
            
            // Increase hit points (simplified - in real D&D this varies by class)
            var hitPointIncrease = 1 + character.ConstitutionModifier; // Minimum 1 HP per level
            character.MaxHealth += Math.Max(1, hitPointIncrease);
            character.Health += Math.Max(1, hitPointIncrease); // Heal on level up
            
            character.UpdatedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Character {CharacterName} leveled up! New level: {Level}, HP: {Health}/{MaxHealth}", 
                character.Name, character.Level, character.Health, character.MaxHealth);
        }
        
        private static int CalculateExperienceForNextLevel(int currentLevel)
        {
            // Simplified D&D 5e experience progression
            return currentLevel switch
            {
                1 => 300,
                2 => 900,
                3 => 2700,
                4 => 6500,
                5 => 14000,
                6 => 23000,
                7 => 34000,
                8 => 48000,
                9 => 64000,
                10 => 85000,
                11 => 100000,
                12 => 120000,
                13 => 140000,
                14 => 165000,
                15 => 195000,
                16 => 225000,
                17 => 265000,
                18 => 305000,
                19 => 355000,
                _ => 0 // Level 20 is max
            };
        }
    }
    
    // ===== MONSTER REPOSITORY IMPLEMENTATION =====
    
    /// <summary>
    /// Monster repository for combat encounters
    /// </summary>
    public class MonsterRepository : Repository<Monster>, IMonsterRepository
    {
        public MonsterRepository(ApplicationDbContext context, ILogger<MonsterRepository> logger) 
            : base(context, logger) { }
        
        public async Task<IEnumerable<Monster>> GetByChallengeRatingAsync(decimal challengeRating)
        {
            _logger.LogDebug("Getting monsters with challenge rating {CR}", challengeRating);
            return await _dbSet
                .Where(m => m.ChallengeRating == challengeRating)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Monster>> GetByTypeAsync(string monsterType)
        {
            _logger.LogDebug("Getting monsters of type {Type}", monsterType);
            return await _dbSet
                .Where(m => m.Type == monsterType)
                .OrderBy(m => m.ChallengeRating)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Monster>> GetForCharacterLevelAsync(int characterLevel)
        {
            _logger.LogDebug("Getting monsters suitable for character level {Level}", characterLevel);
            
            // Simple CR calculation: appropriate monsters are roughly CR = Level/4 to Level/2
            var minCR = Math.Max(0.125m, characterLevel / 8m);
            var maxCR = Math.Max(0.25m, characterLevel / 2m);
            
            return await _dbSet
                .Where(m => m.ChallengeRating >= minCR && m.ChallengeRating <= maxCR)
                .OrderBy(m => m.ChallengeRating)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }
        
        public async Task<Monster?> GetRandomMonsterAsync(decimal? maxChallengeRating = null)
        {
            _logger.LogDebug("Getting random monster with max CR {MaxCR}", maxChallengeRating ?? 999);
            
            var query = _dbSet.AsQueryable();
            if (maxChallengeRating.HasValue)
            {
                query = query.Where(m => m.ChallengeRating <= maxChallengeRating.Value);
            }
            
            var monsters = await query.ToListAsync();
            if (!monsters.Any()) return null;
            
            var random = new Random();
            return monsters[random.Next(monsters.Count)];
        }
        
        public async Task<IEnumerable<Monster>> SearchAsync(string searchTerm)
        {
            _logger.LogDebug("Searching monsters for term: {SearchTerm}", searchTerm);
            
            var lowerSearch = searchTerm.ToLower();
            return await _dbSet
                .Where(m => m.Name.ToLower().Contains(lowerSearch) || 
                           m.Type.ToLower().Contains(lowerSearch))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
    }
}