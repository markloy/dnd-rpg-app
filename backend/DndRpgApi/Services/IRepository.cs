using DndRpgApi.Models;
using System.Linq.Expressions;

namespace DndRpgApi.Services
{
    // ===== GENERIC REPOSITORY INTERFACE =====
    
    /// <summary>
    /// Generic repository interface for common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        // ===== QUERY OPERATIONS =====
        
        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        
        /// <summary>
        /// Find entities matching criteria
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Get first entity matching criteria
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Check if any entity matches criteria
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Get count of entities matching criteria
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        
        /// <summary>
        /// Get paged results
        /// </summary>
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
        
        // ===== MODIFICATION OPERATIONS =====
        
        /// <summary>
        /// Add new entity
        /// </summary>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);
        
        /// <summary>
        /// Update entity
        /// </summary>
        Task<T> UpdateAsync(T entity);
        
        /// <summary>
        /// Delete entity by ID
        /// </summary>
        Task<bool> DeleteAsync(int id);
        
        /// <summary>
        /// Delete entity
        /// </summary>
        Task<bool> DeleteAsync(T entity);
        
        /// <summary>
        /// Delete multiple entities
        /// </summary>
        Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate);
        
        // ===== TRANSACTION OPERATIONS =====
        
        /// <summary>
        /// Save all changes
        /// </summary>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Execute in transaction
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
    }
    
    // ===== SPECIFIC REPOSITORY INTERFACES =====
    
    /// <summary>
    /// Character-specific repository operations
    /// </summary>
    public interface ICharacterRepository : IRepository<Character>
    {
        // ===== CHARACTER-SPECIFIC QUERIES =====
        
        /// <summary>
        /// Get characters by user ID
        /// </summary>
        Task<IEnumerable<Character>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Get characters by level range
        /// </summary>
        Task<IEnumerable<Character>> GetByLevelRangeAsync(int minLevel, int maxLevel);
        
        /// <summary>
        /// Get characters by class
        /// </summary>
        Task<IEnumerable<Character>> GetByClassAsync(string characterClass);
        
        /// <summary>
        /// Search characters by name
        /// </summary>
        Task<IEnumerable<Character>> SearchByNameAsync(string nameSearch);
        
        /// <summary>
        /// Get characters with equipment
        /// </summary>
        Task<Character?> GetWithEquipmentAsync(int characterId);
        
        /// <summary>
        /// Get characters ready to level up
        /// </summary>
        Task<IEnumerable<Character>> GetReadyToLevelUpAsync();
        
        // ===== CHARACTER-SPECIFIC OPERATIONS =====
        
        /// <summary>
        /// Add experience to character
        /// </summary>
        Task<Character> AddExperienceAsync(int characterId, int experience);
        
        /// <summary>
        /// Heal character
        /// </summary>
        Task<Character> HealCharacterAsync(int characterId, int healAmount);
        
        /// <summary>
        /// Damage character
        /// </summary>
        Task<Character> DamageCharacterAsync(int characterId, int damage);
        
        /// <summary>
        /// Level up character
        /// </summary>
        Task<Character> LevelUpAsync(int characterId);
        
        /// <summary>
        /// Check if character name is available
        /// </summary>
        Task<bool> IsNameAvailableAsync(string name, int? excludeCharacterId = null);
    }
    
    /// <summary>
    /// Monster repository for combat encounters
    /// </summary>
    public interface IMonsterRepository : IRepository<Monster>
    {
        // ===== MONSTER-SPECIFIC QUERIES =====
        
        /// <summary>
        /// Get monsters by challenge rating
        /// </summary>
        Task<IEnumerable<Monster>> GetByChallengeRatingAsync(decimal challengeRating);
        
        /// <summary>
        /// Get monsters by type (e.g., Dragon, Undead)
        /// </summary>
        Task<IEnumerable<Monster>> GetByTypeAsync(string monsterType);
        
        /// <summary>
        /// Get monsters suitable for character level
        /// </summary>
        Task<IEnumerable<Monster>> GetForCharacterLevelAsync(int characterLevel);
        
        /// <summary>
        /// Get random monster for encounter
        /// </summary>
        Task<Monster?> GetRandomMonsterAsync(decimal? maxChallengeRating = null);
        
        /// <summary>
        /// Search monsters by name or type
        /// </summary>
        Task<IEnumerable<Monster>> SearchAsync(string searchTerm);
    }
    
    // ===== HELPER CLASSES =====
    
    /// <summary>
    /// Paged result wrapper
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
    
    /// <summary>
    /// Repository operation result
    /// </summary>
    public class RepositoryResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        
        public static RepositoryResult<T> SuccessResult(T data) => new()
        {
            Success = true,
            Data = data
        };
        
        public static RepositoryResult<T> ErrorResult(string message, Exception? exception = null) => new()
        {
            Success = false,
            ErrorMessage = message,
            Exception = exception
        };
    }
}