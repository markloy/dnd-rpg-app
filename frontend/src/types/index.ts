// src/types/index.ts

// Character-related types
export interface Character {
  id: string;
  name: string;
  level: number;
  health: number;
  maxHealth: number;
  experience: number;              // Current level XP progress (0 to experienceToNextLevel)
  experienceToNextLevel: number;   // XP needed to complete current level
  totalExperience: number;         // NEW: Total XP earned (for D&D 5e calculations)
  gold: number;
  stats: CharacterStats;
  equipment: Equipment;
}

export interface CharacterStats {
  strength: number;
  dexterity: number;
  constitution: number;
  intelligence: number;
  wisdom: number;
  charisma: number;
}

export interface Equipment {
  weapon: Weapon;
  armor: Armor;
  shield: Shield;
}

export interface Weapon {
  name: string;
  damage: string; // e.g., "1d8"
  attackBonus: number;
}

export interface Armor {
  name: string;
  armorClass: number;
}

export interface Shield {
  name: string;
  armorClassBonus: number;
}

// Monster-related types
export interface Monster {
  id: string;
  name: string;
  health: number;
  maxHealth: number;
  armorClass: number;
  attackBonus: number;
  damage: string;
  experienceReward: number;
  image?: string;
  type: string;
}

// Combat-related types
export interface CombatLog {
  id: string;
  message: string;
  timestamp: Date;
  type: 'player-attack' | 'monster-attack' | 'system' | 'damage' | 'miss';
}

export interface AttackResult {
  hit: boolean;
  damage: number;
  roll: number;
  totalAttack: number;
}

// Game state types
export interface GameState {
  character: Character;
  currentMonster: Monster | null;
  combatLog: CombatLog[];
  isInCombat: boolean;
  gamePhase: 'encounter' | 'combat' | 'victory' | 'defeat' | 'shopping';
}

// Shop-related types
export interface ShopItem {
  id: string;
  name: string;
  price: number;
  type: 'weapon' | 'armor' | 'shield' | 'consumable';
  description: string;
}

// ===== D&D 5e Experience System Types =====

// Official D&D 5e Experience Point Requirements
export const DND5E_XP_TABLE: Record<number, number> = {
  1: 0,
  2: 300,
  3: 900,
  4: 2700,
  5: 6500,
  6: 14000,
  7: 23000,
  8: 34000,
  9: 48000,
  10: 64000,
  11: 85000,
  12: 100000,
  13: 120000,
  14: 140000,
  15: 165000,
  16: 195000,
  17: 225000,
  18: 265000,
  19: 305000,
  20: 355000,
};

// Experience progress information
export interface LevelProgress {
  currentLevel: number;
  currentLevelXP: number;
  nextLevelXP: number;
  xpProgress: number;        // XP earned in current level (for display bar)
  xpToNextLevel: number;     // Total XP needed to complete level
  totalXP: number;
  isMaxLevel: boolean;
}