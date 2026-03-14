import React from "react";
import { Link2Icon } from "lucide-react";

interface LogoProps {
  className?: string;
  size?: "sm" | "md" | "lg";
}

export const Logo: React.FC<LogoProps> = ({ className = "", size = "md" }) => {
  const sizeClasses = {
    sm: "text-xl",
    md: "text-3xl",
    lg: "text-5xl",
  };

  const iconSizes = {
    sm: 18,
    md: 28,
    lg: 48,
  };

  return (
    <div className={`flex items-center gap-2 font-bold tracking-tight ${className}`}>
      <div className="bg-indigo-600 p-1.5 rounded-lg text-white">
        <Link2Icon size={iconSizes[size]} />
      </div>
      <div className={sizeClasses[size]}>
        <span className="text-indigo-600">Sen</span>
        <span className="text-slate-700">Link</span>
      </div>
    </div>
  );
};
