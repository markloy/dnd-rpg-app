import React, {JSX} from "react";
import { CharacterStats } from "../types";

interface StatBlockProps {
  stats: CharacterStats;    
    showModifiers?: boolean;
}

function StatBlock({ stats, showModifiers = true }: StatBlockProps): JSX.Element {
    // Calculate modifier for a stat
    const calculateModifier = (value: number): number => {
        return Math.floor((value - 10) / 2);
    }   

    // format a modifer for display
    const formatModifier = (modifier: number): string => {
        return modifier >= 0 ? `+${modifier}` : `${modifier}`;
    }

    // Array of stat names for easy iteration
  const statEntries: Array<{ key: keyof CharacterStats; label: string }> = [
    { key: 'strength', label: 'Strength' },
    { key: 'dexterity', label: 'Dexterity' },
    { key: 'constitution', label: 'Constitution' },
    { key: 'intelligence', label: 'Intelligence' },
    { key: 'wisdom', label: 'Wisdom' },
    { key: 'charisma', label: 'Charisma' }
  ];
  
  return (
    <div className="stat-block">
      <h3>Character Stats</h3>
      <div className="stats-grid">
        {/* Map through each stat to create consistent display */}
        {statEntries.map(({ key, label }: { key: keyof CharacterStats; label: string }) => {
          const score: number = stats[key];
          const modifier: number = calculateModifier(score);
          
          return (
            <div key={key} className="stat-item">
              <div className="stat-label">{label}</div>
              <div className="stat-value">
                <span className="stat-score">{score}</span>
                {showModifiers && (
                  <span className="stat-modifier">
                    ({formatModifier(modifier)})
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>
      
      {/* Derived stats */}
      <div className="derived-stats">
        <h4>Derived Stats</h4>
        <div className="derived-stat">
          <strong>Attack Bonus:</strong> {formatModifier(calculateModifier(stats.strength))}
        </div>
        <div className="derived-stat">
          <strong>AC Dex Bonus:</strong> {formatModifier(calculateModifier(stats.dexterity))}
        </div>
        <div className="derived-stat">
          <strong>HP Bonus per Level:</strong> {formatModifier(calculateModifier(stats.constitution))}
        </div>
      </div>
    </div>
  );
}

export default StatBlock;