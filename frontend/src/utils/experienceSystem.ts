import { Character, DND5E_XP_TABLE, LevelProgress } from '../types';

// ===== Helper Functions =====

/**
 * Calculate character level based on total experience points
 */
export const calculateLevelFromXP = (totalXP: number): number => {
  for (let level = 20; level >= 1; level--) {
    if (totalXP >= DND5E_XP_TABLE[level]) {
      return level;
    }
  }
  return 1; // Minimum level
};

/**
 * Get XP required for a specific level
 */
export const getXPForLevel = (level: number): number => {
  return DND5E_XP_TABLE[Math.min(Math.max(level, 1), 20)] || 0;
};

/**
 * Calculate XP needed to reach the next level
 */
export const getXPForNextLevel = (currentLevel: number): number => {
  if (currentLevel >= 20) return DND5E_XP_TABLE[20]; // Max level
  return DND5E_XP_TABLE[currentLevel + 1];
};

/**
 * Calculate experience progress for current level
 * Works with your existing experience/experienceToNextLevel fields
 */
export const calculateLevelProgress = (totalXP: number): LevelProgress => {
  const currentLevel = calculateLevelFromXP(totalXP);
  const currentLevelXP = getXPForLevel(currentLevel);
  const nextLevelXP = getXPForNextLevel(currentLevel);
  
  // Experience earned in current level (relative to level start)
  const xpProgress = totalXP - currentLevelXP;
  
  // Experience needed to complete current level
  const xpToNextLevel = nextLevelXP - currentLevelXP;
  
  return {
    currentLevel,
    currentLevelXP,
    nextLevelXP,
    xpProgress,        // This becomes your `experience` field
    xpToNextLevel,     // This becomes your `experienceToNextLevel` field
    totalXP,
    isMaxLevel: currentLevel >= 20
  };
};

/**
 * Add experience to character and update ALL experience fields
 * Maintains compatibility with your existing experience/experienceToNextLevel structure
 */
export const addExperienceToCharacter = (character: Character, xpToAdd: number): Character => {
  const newTotalXP = character.totalExperience + xpToAdd;
  const progress = calculateLevelProgress(newTotalXP);
  
  // Check if character leveled up
  const didLevelUp = progress.currentLevel > character.level;
  
  return {
    ...character,                              // Preserves ALL existing properties (equipment, gold, etc.)
    totalExperience: newTotalXP,              // NEW: Total XP earned
    level: progress.currentLevel,              // Updated level
    experience: progress.xpProgress,           // EXISTING: Current level progress
    experienceToNextLevel: progress.xpToNextLevel, // EXISTING: XP needed for level
    // Optional: Heal character on level up
    health: didLevelUp ? character.maxHealth : character.health,
    // Optional: Increase max health on level up
    maxHealth: didLevelUp ? character.maxHealth + 5 : character.maxHealth
  };
};

/**
 * Set character to specific level (useful for testing)
 * Updates all experience-related fields properly
 */
export const setCharacterLevel = (character: Character, targetLevel: number): Character => {
  const clampedLevel = Math.min(Math.max(targetLevel, 1), 20);
  const targetTotalXP = getXPForLevel(clampedLevel);
  const progress = calculateLevelProgress(targetTotalXP);
  
  return {
    ...character,
    totalExperience: targetTotalXP,
    level: clampedLevel,
    experience: progress.xpProgress,           // Will be 0 (start of level)
    experienceToNextLevel: progress.xpToNextLevel,
    health: character.maxHealth,
    maxHealth: character.maxHealth + Math.max(0, (clampedLevel - character.level) * 5)
  };
};

/**
 * Initialize a character with proper experience values
 * Use this when creating new characters
 */
export const initializeCharacterExperience = (character: Omit<Character, 'totalExperience'>): Character => {
  // If character already has level/experience, calculate totalExperience
  // Otherwise, start at level 1
  const level = character.level || 1;
  const totalXP = getXPForLevel(level) + (character.experience || 0);
  const progress = calculateLevelProgress(totalXP);
  
  return {
    ...character,
    totalExperience: totalXP,
    level: progress.currentLevel,
    experience: progress.xpProgress,
    experienceToNextLevel: progress.xpToNextLevel
  };
};

/**
 * Convert your existing character to the new system
 * Call this once to add totalExperience to existing characters
 */
export const migrateCharacterToNewSystem = (character: Character): Character => {
  // Calculate totalExperience from current level and experience
  const baseLevelXP = getXPForLevel(character.level);
  const totalXP = baseLevelXP + (character.experience || 0);
  
  return {
    ...character,
    totalExperience: totalXP
  };
};