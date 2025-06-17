import React, {JSX} from "react";

interface HealthBarProps {
  currentHealth: number;
  maxHealth: number;
  label?: string;
  showNumbers?: boolean;
}
function HealthBar({
  currentHealth,
  maxHealth,
  label = "Health",
  showNumbers = true,
}: HealthBarProps): JSX.Element {
  const healthPercentage: number = (currentHealth / maxHealth) * 100;
  
  // Determine bar color based on health percentage
  const getHealthColor = (percentage: number): string => {
    if (percentage > 75) return '#4CAF50'; // Green
    if (percentage > 50) return '#FF9800'; // Orange  
    if (percentage > 25) return '#F44336'; // Red
    return '#B71C1C'; // Dark red
  };

  return (
    <div className="health-bar-container">
      {/* Label */}
      <div className="health-label">
        <strong>{label}</strong>
        {showNumbers && (
          <span className="health-numbers">
            {currentHealth} / {maxHealth}
          </span>
        )}
      </div>
      
      {/* Health bar */}
      <div className="health-bar-background">
        <div 
          className="health-bar-fill"
          style={{
            width: `${Math.max(0, healthPercentage)}%`,
            backgroundColor: getHealthColor(healthPercentage),
            transition: 'width 0.3s ease-in-out' // Smooth animation
          }}
        />
      </div>
      
      {/* Health percentage (only show if very low) */}
      {healthPercentage < 25 && (
        <div className="health-warning">
          ⚠️ Critical Health!
        </div>
      )}
    </div>
  );
}

export default HealthBar;