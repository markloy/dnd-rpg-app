import React, { JSX } from "react";
import { Character } from "../types";
import "../App.css"

interface ExperienceBarProps {
  character: Character;  // Uses your existing Character interface
  showNumbers?: boolean;
  label?: string;
  showLevel?: boolean;
}

function ExperienceBar({
  character,
  showNumbers = true,
  label = "Experience",
  showLevel = true,
}: ExperienceBarProps): JSX.Element {
  
  // Use your existing experience fields directly!
  const experiencePercentage: number = character.level >= 20 
    ? 100 
    : Math.min((character.experience / character.experienceToNextLevel) * 100, 100);

  // Determine bar color based on experience progress percentage
  const getExperienceColor = (percentage: number): string => {
    if (character.level >= 20) return '#ffd700'; // Gold for max level
    if (percentage > 75) return '#17f803';       // Bright green
    if (percentage > 50) return '#109e03';       // Medium green  
    if (percentage > 25) return '#0a6301';       // Dark green
    return '#063002';                            // Darker green
  };

  const isMaxLevel = character.level >= 20;

  return (
    <div className="experience-bar-container">
      {/* Label and Level */}
      <div className="experience-label">
        <strong>
          {showLevel && `Level ${character.level} - `}
          {label}
        </strong>
        
        {showNumbers && (
          <span className="experience-numbers">
            {isMaxLevel ? (
              <span className="max-level">MAX LEVEL ({character.totalExperience?.toLocaleString() || 'Unknown'} XP)</span>
            ) : (
              <>
                {character.experience.toLocaleString()} / {character.experienceToNextLevel.toLocaleString()}
                <span className="experience-needed">
                  {" "}({(character.experienceToNextLevel - character.experience).toLocaleString()} needed)
                </span>
              </>
            )}
          </span>
        )}
      </div>

      {/* Experience bar */}
      <div className="experience-bar-background">
        <div
          className="experience-bar-fill"
          style={{
            width: `${Math.max(0, experiencePercentage)}%`,
            backgroundColor: getExperienceColor(experiencePercentage),
            transition: "width 0.3s ease-in-out",
            animation: experiencePercentage > 90 && !isMaxLevel 
              ? "pulse 1s infinite" 
              : undefined,
          }}
        />
      </div>
      
      {/* Experience warnings and notifications */}
      {isMaxLevel && (
        <div className="max-level-notification">
          🏆 Maximum Level Reached!
        </div>
      )}
      
      {!isMaxLevel && experiencePercentage > 95 && (
        <div className="experience-warning">
          ⚠️ Just About To Level Up!
        </div>
      )}
    </div>
  );
}

export default ExperienceBar;