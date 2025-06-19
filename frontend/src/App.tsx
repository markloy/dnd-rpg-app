
import React, { useState, useEffect } from 'react';
import './App.css';

import { Character } from './types';
import { addExperienceToCharacter, setCharacterLevel } from './utils/experienceSystem';

import ExperienceBar from './components/ExperienceBar';
import HealthBar from './components/HealthBar';
import StatBlock from './components/StatBlock';

import { ApiService } from './services/apiService';

function App(): React.JSX.Element {
  
  const [character, setCharacter] = useState<Character | null>(null);
  
  const [characters, setCharacters] = useState<Character[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [apiStatus, setApiStatus] = useState<'unknown' | 'connected' | 'error'>('unknown');

  useEffect(() => {
    const testBackendConnection = async () => {
      try {
        console.log('🔄 Testing backend connection...');
        
        // Try to connect to backend
        const healthResult = await ApiService.checkHealth();
        console.log('✅ Backend connected:', healthResult);
        
        const dbResult = await ApiService.testDatabase();
        console.log('✅ Database connected:', dbResult);
        
        setApiStatus('connected');
        await loadCharactersFromBackend();
        
      } catch (error) {
        console.error('❌ Backend connection failed:', error);
        setApiStatus('error');
        setError('Could not connect to backend API. Using local character data.');
        
        setCharacter(getYourExistingCharacter());
      } finally {
        setLoading(false);
      }
    };

    testBackendConnection();
  }, []);

  const loadCharactersFromBackend = async (): Promise<void> => {
    try {
      setLoading(true);
      const charactersData = await ApiService.getCharacters();
      setCharacters(charactersData);
      
      if (charactersData.length > 0) {
        setCharacter(charactersData[0]);
        console.log('✅ Loaded characters from backend:', charactersData);
      } else {
        console.log('ℹ️ No characters in backend - using your existing character');
        setCharacter(getYourExistingCharacter());
      }
    } catch (error) {
      console.error('Failed to load characters:', error);
      setError('Could not load characters from backend');
      setCharacter(getYourExistingCharacter());
    } finally {
      setLoading(false);
    }
  };

  const getYourExistingCharacter = (): Character => ({
    id: "local-1", 
    name: "Aragorn",
    level: 1,
    health: 100,
    maxHealth: 100,
    experience: 0,
    experienceToNextLevel: 300,
    totalExperience: 0,
    gold: 100,
    stats: {
      strength: 16,
      dexterity: 14,
      constitution: 15,
      intelligence: 12,
      wisdom: 13,
      charisma: 10
    },
    equipment: {
      weapon: {
        name: "Longsword",
        damage: "1d8",
        attackBonus: 3
      },
      armor: {
        name: "Chainmail",
        armorClass: 16
      },
      shield: {
        name: "Shield",
        armorClassBonus: 2
      }
    }
  });

  const handleHeal = async (event: React.MouseEvent<HTMLButtonElement>): Promise<void> => {
    if (!character) return;

    try {
      if (character.id.startsWith('local-') || apiStatus !== 'connected') {
        setCharacter(prevCharacter => {
          if (!prevCharacter) return null; // Handle null case
          
          return {
            ...prevCharacter,
            health: Math.min(prevCharacter.maxHealth, prevCharacter.health + 20)
          };
        });
        console.log('Character healed! (local mode)');
        
      } else {
        // Use backend API for real characters
        const updatedCharacter = await ApiService.healCharacter(parseInt(character.id), 20);
        setCharacter(updatedCharacter);
        console.log('✅ Character healed via backend!');
      }
    } catch (error) {
      console.error('Failed to heal character:', error);
      setError('Could not heal character');
    }
  };

  const handleExperienceBoost = async (event: React.MouseEvent<HTMLButtonElement>): Promise<void> => {
    if (!character) return;

    try {
      if (character.id.startsWith('local-') || apiStatus !== 'connected') {
        setCharacter(prevCharacter => {
          if (!prevCharacter) return null; // Handle null case
          
          const newCharacter = addExperienceToCharacter(prevCharacter, 100);
          
          if (newCharacter.level > prevCharacter.level) {
            console.log(`🎉 LEVEL UP! ${prevCharacter.name} reached level ${newCharacter.level}!`);
            console.log(`New HP: ${newCharacter.health}/${newCharacter.maxHealth}`);
          }
          
          return newCharacter;
        });
        console.log('Experience gained! (+100 XP) (local mode)');
        
      } else {
        // Use backend API for real characters
        const updatedCharacter = await ApiService.addExperience(parseInt(character.id), 100);
        
        if (updatedCharacter.level > character.level) {
          console.log(`🎉 LEVEL UP! ${character.name} reached level ${updatedCharacter.level}!`);
        }
        
        setCharacter(updatedCharacter);
        console.log('✅ Experience gained via backend! (+100 XP)');
      }
    } catch (error) {
      console.error('Failed to add experience:', error);
      setError('Could not add experience');
    }
  };

  const handleBigExperienceBoost = async (event: React.MouseEvent<HTMLButtonElement>): Promise<void> => {
    if (!character) return;

    try {
      if (character.id.startsWith('local-') || apiStatus !== 'connected') {
        setCharacter(prevCharacter => {
          if (!prevCharacter) return null; 
          
          const newCharacter = addExperienceToCharacter(prevCharacter, 1000);
          
          if (newCharacter.level > prevCharacter.level) {
            console.log(`🚀 BIG LEVEL JUMP! ${prevCharacter.name} jumped to level ${newCharacter.level}!`);
          }
          
          return newCharacter;
        });
        console.log('Big experience boost! (+1000 XP) (local mode)');
        
      } else {
        const updatedCharacter = await ApiService.addExperience(parseInt(character.id), 1000);
        
        if (updatedCharacter.level > character.level) {
          console.log(`🚀 BIG LEVEL JUMP! ${character.name} jumped to level ${updatedCharacter.level}!`);
        }
        
        setCharacter(updatedCharacter);
        console.log('✅ Big experience boost via backend! (+1000 XP)');
      }
    } catch (error) {
      console.error('Failed to add big experience:', error);
      setError('Could not add experience');
    }
  };

  const handleSetLevel = (targetLevel: number) => (): void => {
    if (!character) return;

    if (character.id.startsWith('local-') || apiStatus !== 'connected') {
      setCharacter(prevCharacter => {
        if (!prevCharacter) return null; 
        
        return setCharacterLevel(prevCharacter, targetLevel);
      });
      console.log(`Set character to level ${targetLevel} (local mode)`);
    } else {
      console.log('Level setting not implemented for backend characters yet');
      setError('Level setting only works in local mode currently');
    }
  };

  const handleCharacterSelect = (selectedCharacter: Character): void => {
    setCharacter(selectedCharacter);
    console.log('Switched to character:', selectedCharacter.name);
  };

  if (loading) {
    return (
      <div className="App">
        <div className="character-sheet">
          <h1>🐉 D&D RPG Character Manager</h1>
          <p>Loading characters...</p>
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <div style={{ 
              border: '4px solid #f3f3f3',
              borderTop: '4px solid #FFD700',
              borderRadius: '50%',
              width: '40px',
              height: '40px',
              animation: 'spin 2s linear infinite',
              margin: '0 auto'
            }}></div>
          </div>
        </div>
      </div>
    );
  }

  if (!character) {
    return (
      <div className="App">
        <div className="character-sheet">
          <h1>🐉 D&D RPG Character Manager</h1>
          <p>No character available</p>
          <button 
            onClick={() => setCharacter(getYourExistingCharacter())}
            className="btn btn--primary"
          >
            Load Default Character
          </button>
        </div>
      </div>
    );
  }

  const totalArmorClass = character.equipment.armor.armorClass + character.equipment.shield.armorClassBonus;

  return (
    <div className="App">
      <div className="character-sheet">
        
        <div style={{ 
          textAlign: 'center', 
          marginBottom: '1rem',
          padding: '0.5rem',
          backgroundColor: apiStatus === 'connected' ? '#1e4d1e' : '#4d1e1e',
          borderRadius: '4px',
          border: `1px solid ${apiStatus === 'connected' ? '#4CAF50' : '#ff6b6b'}`
        }}>
          {apiStatus === 'connected' ? (
            <span style={{ color: '#4CAF50' }}>✅ Connected to Backend API</span>
          ) : (
            <span style={{ color: '#ff6b6b' }}>⚠️ Using Local Data (Backend Disconnected)</span>
          )}
        </div>

        {characters.length > 1 && (
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem', color: '#FFD700' }}>
              Select Character:
            </label>
            <select 
              value={character.id} 
              onChange={(e) => {
                const selected = characters.find(c => c.id === e.target.value);
                if (selected) handleCharacterSelect(selected);
              }}
              style={{
                padding: '0.5rem',
                borderRadius: '4px',
                border: '1px solid #555',
                background: '#333',
                color: '#fff',
                width: '100%'
              }}
            >
              {characters.map(char => (
                <option key={char.id} value={char.id}>
                  {char.name} (Level {char.level})
                </option>
              ))}
            </select>
          </div>
        )}

        <h1>{character.name}</h1>
        <p><strong>Level {character.level}</strong> | <strong>Gold:</strong> {character.gold}</p>
        
        <div className="character-stats">
          <h3>Stats</h3>
          <div className="stats-grid">
            <span><strong>STR:</strong> {character.stats.strength}</span>
            <span><strong>DEX:</strong> {character.stats.dexterity}</span>
            <span><strong>CON:</strong> {character.stats.constitution}</span>
            <span><strong>INT:</strong> {character.stats.intelligence}</span>
            <span><strong>WIS:</strong> {character.stats.wisdom}</span>
            <span><strong>CHA:</strong> {character.stats.charisma}</span>
          </div>
        </div>

        <HealthBar 
          currentHealth={character.health}
          maxHealth={character.maxHealth}
          showNumbers={true}
        />

        <div className="combat-stats">
          <p><strong>Health:</strong> {character.health} / {character.maxHealth}</p>
          <p><strong>Armor Class:</strong> {totalArmorClass}</p>
        </div>

        <div className="equipment-display">
          <h3>Equipment</h3>
          <p><strong>Weapon:</strong> {character.equipment.weapon.name} ({character.equipment.weapon.damage}, +{character.equipment.weapon.attackBonus})</p>
          <p><strong>Armor:</strong> {character.equipment.armor.name} (AC {character.equipment.armor.armorClass})</p>
          <p><strong>Shield:</strong> {character.equipment.shield.name} (+{character.equipment.shield.armorClassBonus} AC)</p>
        </div>
        
        <ExperienceBar 
          character={character}
          showNumbers={true}
          showLevel={true}
        />
        
        <div className="action-buttons">
          <button onClick={handleHeal} className="heal-button">
            Heal (+20 HP)
          </button>
          
          <button onClick={handleExperienceBoost} className="xp-button">
            Gain Experience (+100 XP)
          </button>
          
          <button onClick={handleBigExperienceBoost} className="big-xp-button">
            Big XP Boost (+1000 XP)
          </button>
        </div>

        <div className="dev-tools">
          <h4>🛠️ Development Tools</h4>
          <div className="level-test-buttons">
            <button onClick={handleSetLevel(1)}>Level 1</button>
            <button onClick={handleSetLevel(3)}>Level 3</button>
            <button onClick={handleSetLevel(5)}>Level 5</button>
            <button onClick={handleSetLevel(10)}>Level 10</button>
            <button onClick={handleSetLevel(20)}>Level 20</button>
          </div>
          
          <div className="debug-info">
            <small>
              <strong>Debug:</strong> Total XP: {character.totalExperience} | 
              Current Level XP: {character.experience}/{character.experienceToNextLevel} |
              Mode: {character.id.startsWith('local-') ? 'Local' : 'Backend'} |
              API: {apiStatus}
            </small>
          </div>
          
          {error && (
            <div style={{ 
              marginTop: '0.5rem', 
              padding: '0.5rem', 
              background: '#4d1e1e', 
              borderRadius: '4px',
              border: '1px solid #ff6b6b'
            }}>
              <small style={{ color: '#ff6b6b' }}>
                <strong>Status:</strong> {error}
              </small>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;