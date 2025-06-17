import React, {JSX} from "react";

interface ButtonProps {
  children: React.ReactNode;
  onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;  // ← Fixed: added missing >
  disabled?: boolean;
  variant?: "primary" | "secondary" | "danger" | "success";
  size?: "small" | "medium" | "large";
  className?: string;
}

function Button({
  children,
  onClick,
  disabled = false,
  variant = "primary",
  size = "medium",
  className = "",
}: ButtonProps): JSX.Element {
  // Define base styles
  const baseClasses: string = `btn`;
  const variantClass: string = `btn--${variant}`;
  const sizeClass: string = `btn--${size}`;
  const disabledClass: string = disabled ? 'btn--disabled' : '';

  // Combine all classes
  const allClasses: string = [
    baseClasses,
    variantClass, 
    sizeClass,
    disabledClass,
    className
  ].filter(Boolean).join(' '); // Remove empty strings and join with spaces

  // Handle click events - don't trigger if disabled
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>): void => {
    if (!disabled) {
      onClick(event);
    }
  };

   return (
    <button 
      className={allClasses}
      onClick={handleClick}
      disabled={disabled}
      type="button" // Prevent form submission by default
    >
      {children}
    </button>
  );
}

export default Button;