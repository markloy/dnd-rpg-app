import React, { useState } from 'react';
import './App.css';

// Import your existing types and new experience functions
import { Character } from './types';
import { addExperienceToCharacter, setCharacterLevel } from './utils/experienceSystem';
import ExperienceBar from './components/ExperienceBar';

function App(): React.JSX.Element {
  // Character state using YOUR existing structure + totalExperience
  const [character, setCharacter] = useState<Character>({
    id: "1",
    name: "Aragorn",
    level: 1,
    health: 100,
    maxHealth: 100,
    experience: 0,                    // Current level progress (existing field)
    experienceToNextLevel: 300,       // XP needed for current level (existing field)
    totalExperience: 0,              // NEW: Total XP earned (D&D 5e tracking)
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

  // Handle the heal button (existing functionality)
  const handleHeal = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setCharacter((prevCharacter: Character): Character => ({
      ...prevCharacter,
      health: Math.min(prevCharacter.maxHealth, prevCharacter.health + 20)
    }));
    console.log('Character healed!');
  };

  // NEW: Handle experience boost using D&D 5e system
  const handleExperienceBoost = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setCharacter(prevCharacter => {
      const newCharacter = addExperienceToCharacter(prevCharacter, 100);
      
      // Check if leveled up and show celebration
      if (newCharacter.level > prevCharacter.level) {
        console.log(`🎉 LEVEL UP! ${prevCharacter.name} reached level ${newCharacter.level}!`);
        console.log(`New HP: ${newCharacter.health}/${newCharacter.maxHealth}`);
      }
      
      return newCharacter;
    });
    console.log('Experience gained! (+100 XP)');
  };

  // Big experience boost for testing different levels
  const handleBigExperienceBoost = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setCharacter(prevCharacter => {
      const newCharacter = addExperienceToCharacter(prevCharacter, 1000);
      
      if (newCharacter.level > prevCharacter.level) {
        console.log(`🚀 BIG LEVEL JUMP! ${prevCharacter.name} jumped to level ${newCharacter.level}!`);
      }
      
      return newCharacter;
    });
    console.log('Big experience boost! (+1000 XP)');
  };

  // Level testing buttons for development
  const handleSetLevel = (targetLevel: number) => (): void => {
    setCharacter(prevCharacter => setCharacterLevel(prevCharacter, targetLevel));
    console.log(`Set character to level ${targetLevel}`);
  };

  // Calculate total armor class
  const totalArmorClass = character.equipment.armor.armorClass + character.equipment.shield.armorClassBonus;

  return (
    <div className="App">
      <div className="character-sheet">
        {/* Character Header */}
        <h1>{character.name}</h1>
        <p><strong>Level {character.level}</strong> | <strong>Gold:</strong> {character.gold}</p>
        
        {/* Character Stats */}
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

        {/* Health and Combat Stats */}
        <div className="combat-stats">
          <p><strong>Health:</strong> {character.health} / {character.maxHealth}</p>
          <p><strong>Armor Class:</strong> {totalArmorClass}</p>
        </div>

        {/* Equipment Display */}
        <div className="equipment-display">
          <h3>Equipment</h3>
          <p><strong>Weapon:</strong> {character.equipment.weapon.name} ({character.equipment.weapon.damage}, +{character.equipment.weapon.attackBonus})</p>
          <p><strong>Armor:</strong> {character.equipment.armor.name} (AC {character.equipment.armor.armorClass})</p>
          <p><strong>Shield:</strong> {character.equipment.shield.name} (+{character.equipment.shield.armorClassBonus} AC)</p>
        </div>
        
        {/* NEW: Experience Bar using your existing fields */}
        <ExperienceBar 
          character={character}
          showNumbers={true}
          showLevel={true}
        />
        
        {/* Action Buttons */}
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

        {/* Development Tools - Remove in production */}
        <div className="dev-tools">
          <h4>🛠️ Development Tools</h4>
          <div className="level-test-buttons">
            <button onClick={handleSetLevel(1)}>Level 1</button>
            <button onClick={handleSetLevel(3)}>Level 3</button>
            <button onClick={handleSetLevel(5)}>Level 5</button>
            <button onClick={handleSetLevel(10)}>Level 10</button>
            <button onClick={handleSetLevel(20)}>Level 20</button>
          </div>
          
          {/* Debug Info */}
          <div className="debug-info">
            <small>
              <strong>Debug:</strong> Total XP: {character.totalExperience} | 
              Current Level XP: {character.experience}/{character.experienceToNextLevel}
            </small>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;