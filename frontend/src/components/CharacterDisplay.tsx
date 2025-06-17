import React, { JSX } from "react";
import { Character } from "../types/index";

interface CharacterDisplayProps {
  character: Character;
}

function CharacterDisplay({ character }: CharacterDisplayProps): JSX.Element {
  // calculate armor class from equipment
  const totalArmorClass: number =
    character.equipment.armor.armorClass +
    (character.equipment.shield
      ? character.equipment.shield.armorClassBonus
      : 0);
  return (
    <div className="character-display">
      <h2>{character.name}</h2>

      {/* Health Display with visual bar */}
      <div className="health-section">
        <h3>Health</h3>
        <div className="health-bar">
          <div
            className="health-fill"
            style={{
              width: `${(character.health / character.maxHealth) * 100}%`,
              backgroundColor:
                character.health > character.maxHealth * 0.5 ? "green" : "red",
            }}
          />
        </div>
        <span>
          {character.health} / {character.maxHealth}
        </span>
      </div>

      {/* Character progression */}
      <div className="progression-section">
        <p>
          <strong>Level:</strong> {character.level}
        </p>
        <p>
          <strong>Experience:</strong> {character.experience} /{" "}
          {character.experienceToNextLevel}
        </p>
        <p>
          <strong>Gold:</strong> {character.gold} GP
        </p>
      </div>
      {/* Equipment display */}
      <div className="equipment-section">
        <h3>Equipment</h3>
        <p>
          <strong>Weapon:</strong> {character.equipment.weapon.name} (
          {character.equipment.weapon.damage})
        </p>
        <p>
          <strong>Armor:</strong> {character.equipment.armor.name}
        </p>
        <p>
          <strong>Shield:</strong> {character.equipment.shield.name}
        </p>
        <p>
          <strong>Total AC:</strong> {totalArmorClass}
        </p>
      </div>
    </div>
  );
}

export default CharacterDisplay;
