// src/services/apiService.ts - CORRECTED VERSION WITH PROPER TYPES

import axios, { AxiosResponse } from 'axios';
import { Character, Monster } from '../types';

// ===== API CONFIGURATION =====
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7043/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  }
});

// ===== PROPER RESPONSE TYPE DEFINITIONS =====

// ✅ GOOD - Actual structure from ASP.NET Core health checks
interface HealthCheckResponse {
  status: 'Healthy' | 'Unhealthy' | 'Degraded';
  totalDuration: string;  // Format: "00:00:00.0001234"
  entries: Record<string, {
    status: 'Healthy' | 'Unhealthy' | 'Degraded';
    duration: string;
    description?: string;
    data?: Record<string, unknown>;
  }>;
}

// ✅ GOOD - Based on what our backend actually returns
interface DatabaseTestResponse {
  status: string;
  message: string;
  characterCount: number;
  monsterCount: number;
  connectionString: string;
  timestamp: string;
}

// ✅ GOOD - Standard API error response
interface ApiErrorResponse {
  message: string;
  statusCode: number;
  details?: string;
}

// ===== BACKEND CHARACTER TYPES (Already properly typed) =====
export interface BackendCharacter {
  id: number;
  name: string;
  level: number;
  health: number;
  maxHealth: number;
  experience: number;
  experienceToNextLevel: number;
  totalExperience: number;
  gold: number;
  userId: string;
  characterClass: string;
  background: string;
  race: string;
  strength: number;
  dexterity: number;
  constitution: number;
  intelligence: number;
  wisdom: number;
  charisma: number;
  weaponName: string;
  weaponDamage: string;
  weaponAttackBonus: number;
  armorName: string;
  armorClass: number;
  shieldName: string;
  shieldArmorClassBonus: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCharacterRequest {
  name: string;
  characterClass: string;
  background: string;
  race: string;
  strength: number;
  dexterity: number;
  constitution: number;
  intelligence: number;
  wisdom: number;
  charisma: number;
}

export interface BackendMonster {
  id: number;
  name: string;
  type: string;
  challengeRating: number;
  experienceValue: number;
  hitPoints: number;
  armorClass: number;
  actions: string;
  description?: string;
}

// ===== DATA CONVERSION FUNCTIONS (Same as before) =====
export const convertBackendToFrontendCharacter = (backendChar: BackendCharacter): Character => {
  return {
    id: backendChar.id.toString(),
    name: backendChar.name,
    level: backendChar.level,
    health: backendChar.health,
    maxHealth: backendChar.maxHealth,
    experience: backendChar.experience,
    experienceToNextLevel: backendChar.experienceToNextLevel,
    totalExperience: backendChar.totalExperience,
    gold: backendChar.gold,
    stats: {
      strength: backendChar.strength,
      dexterity: backendChar.dexterity,
      constitution: backendChar.constitution,
      intelligence: backendChar.intelligence,
      wisdom: backendChar.wisdom,
      charisma: backendChar.charisma
    },
    equipment: {
      weapon: {
        name: backendChar.weaponName,
        damage: backendChar.weaponDamage,
        attackBonus: backendChar.weaponAttackBonus
      },
      armor: {
        name: backendChar.armorName,
        armorClass: backendChar.armorClass
      },
      shield: {
        name: backendChar.shieldName || "None",
        armorClassBonus: backendChar.shieldArmorClassBonus
      }
    }
  };
};

export const convertBackendToFrontendMonster = (backendMonster: BackendMonster): Monster => {
  return {
    id: backendMonster.id.toString(),
    name: backendMonster.name,
    health: backendMonster.hitPoints,
    maxHealth: backendMonster.hitPoints,
    armorClass: backendMonster.armorClass,
    attackBonus: 5,
    damage: "1d8+3",
    experienceReward: backendMonster.experienceValue,
    type: backendMonster.type
  };
};

// ===== CORRECTED API SERVICE CLASS =====
export class ApiService {

    static async checkHealthSafe(): Promise<HealthCheckResponse> {
  try {
    const response = await apiClient.get('/health');
    
    // Use type guard for runtime validation
    if (isHealthCheckResponse(response.data)) {
      return response.data;
    }
    
    throw new Error('Invalid health response format');
  } catch (error) {
    if (axios.isAxiosError(error)) {
      // Handle Axios-specific errors
      if (error.response?.status === 503) {
        throw new Error('Service temporarily unavailable');
      }
      if (error.response?.status === 404) {
        throw new Error('Health endpoint not found');
      }
    }
    
    throw new Error('Health check failed');
  }
}
  
  /**
   * Check if the backend API is responding
   */
  static async checkHealth(): Promise<HealthCheckResponse> {
    try {
      // Tell Axios what type to expect
      const response: AxiosResponse<HealthCheckResponse> = await apiClient.get<HealthCheckResponse>('/health');
      return response.data;
    } catch (error) {
      console.error('Health check failed:', error);
      throw new Error('Backend API is not responding');
    }
  }

  /**
   * Test database connectivity
   */
  static async testDatabase(): Promise<DatabaseTestResponse> {
    try {
      const response: AxiosResponse<DatabaseTestResponse> = await apiClient.get<DatabaseTestResponse>('/test-db');
      return response.data;
    } catch (error) {
      console.error('Database test failed:', error);
      throw error;
    }
  }

  /**
   * Get all characters for the current user
   * ✅ ALREADY PROPERLY TYPED
   */
  static async getCharacters(): Promise<Character[]> {
    try {
      const response: AxiosResponse<BackendCharacter[]> = await apiClient.get<BackendCharacter[]>('/characters');
      return response.data.map(convertBackendToFrontendCharacter);
    } catch (error) {
      console.error('Failed to fetch characters:', error);
      throw new Error('Could not load characters');
    }
  }

  /**
   * Get a specific character by ID
   * ✅ ALREADY PROPERLY TYPED
   */
  static async getCharacter(id: number): Promise<Character> {
    try {
      const response: AxiosResponse<BackendCharacter> = await apiClient.get<BackendCharacter>(`/characters/${id}`);
      return convertBackendToFrontendCharacter(response.data);
    } catch (error) {
      console.error(`Failed to fetch character ${id}:`, error);
      throw new Error('Could not load character');
    }
  }

  /**
   * Create a new character
   * ✅ ALREADY PROPERLY TYPED
   */
  static async createCharacter(character: CreateCharacterRequest): Promise<Character> {
    try {
      const response: AxiosResponse<BackendCharacter> = await apiClient.post<BackendCharacter>('/characters', character);
      return convertBackendToFrontendCharacter(response.data);
    } catch (error) {
      console.error('Failed to create character:', error);
      throw new Error('Could not create character');
    }
  }

  /**
   * Add experience points to a character
   * ✅ ALREADY PROPERLY TYPED
   */
  static async addExperience(characterId: number, experience: number): Promise<Character> {
    try {
      const response: AxiosResponse<BackendCharacter> = await apiClient.post<BackendCharacter>(
        `/characters/${characterId}/experience`, 
        experience,
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
      return convertBackendToFrontendCharacter(response.data);
    } catch (error) {
      console.error('Failed to add experience:', error);
      throw new Error('Could not add experience');
    }
  }

  /**
   * Heal a character (restore health points)
   * ✅ ALREADY PROPERLY TYPED
   */
  static async healCharacter(characterId: number, healAmount: number): Promise<Character> {
    try {
      const response: AxiosResponse<BackendCharacter> = await apiClient.post<BackendCharacter>(
        `/characters/${characterId}/heal`, 
        healAmount,
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
      return convertBackendToFrontendCharacter(response.data);
    } catch (error) {
      console.error('Failed to heal character:', error);
      throw new Error('Could not heal character');
    }
  }

  /**
   * Damage a character (reduce health points)
   * ✅ ALREADY PROPERLY TYPED
   */
  static async damageCharacter(characterId: number, damage: number): Promise<Character> {
    try {
      const response: AxiosResponse<BackendCharacter> = await apiClient.post<BackendCharacter>(
        `/characters/${characterId}/damage`, 
        damage,
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
      return convertBackendToFrontendCharacter(response.data);
    } catch (error) {
      console.error('Failed to damage character:', error);
      throw new Error('Could not damage character');
    }
  }

  /**
   * Get all monsters from the database
   * ✅ PROPERLY TYPED (for future implementation)
   */
  static async getMonsters(): Promise<Monster[]> {
    try {
      const response: AxiosResponse<BackendMonster[]> = await apiClient.get<BackendMonster[]>('/monsters');
      return response.data.map(convertBackendToFrontendMonster);
    } catch (error) {
      console.error('Failed to fetch monsters:', error);
      throw new Error('Could not load monsters');
    }
  }

  /**
   * Get a random monster for encounters
   * ✅ PROPERLY TYPED (for future implementation)
   */
  static async getRandomMonster(maxChallengeRating?: number): Promise<Monster> {
    try {
      const params = maxChallengeRating ? `?maxCR=${maxChallengeRating}` : '';
      const response: AxiosResponse<BackendMonster> = await apiClient.get<BackendMonster>(`/monsters/random${params}`);
      return convertBackendToFrontendMonster(response.data);
    } catch (error) {
      console.error('Failed to fetch random monster:', error);
      throw new Error('Could not load monster');
    }
  }
}

// ===== TYPE GUARD UTILITIES (For extra safety) =====

/**
 * Type guard to check if response is valid health check
 */
export function isHealthCheckResponse(data: unknown): data is HealthCheckResponse {
  return (
    typeof data === 'object' &&
    data !== null &&
    'status' in data &&
    typeof (data as HealthCheckResponse).status === 'string' &&
    ['Healthy', 'Unhealthy', 'Degraded'].includes((data as HealthCheckResponse).status)
  );
}

/**
 * Type guard to check if response is valid database test
 */
export function isDatabaseTestResponse(data: unknown): data is DatabaseTestResponse {
  return (
    typeof data === 'object' &&
    data !== null &&
    'status' in data &&
    'characterCount' in data &&
    typeof (data as DatabaseTestResponse).characterCount === 'number'
  );
}

// ===== ENHANCED ERROR HANDLING WITH TYPED ERRORS =====


export default ApiService;