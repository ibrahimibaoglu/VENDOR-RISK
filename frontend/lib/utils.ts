import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";
import type { RiskLevel } from "./types";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Risk level color utilities
export const getRiskColor = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case "Low":
      return "text-green-700 bg-green-50 border-green-200";
    case "Medium":
      return "text-yellow-700 bg-yellow-50 border-yellow-200";
    case "High":
      return "text-orange-700 bg-orange-50 border-orange-200";
    case "Critical":
      return "text-red-700 bg-red-50 border-red-200";
    default:
      return "text-gray-700 bg-gray-50 border-gray-200";
  }
};

export const getRiskBadgeColor = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case "Low":
      return "bg-green-100 text-green-800";
    case "Medium":
      return "bg-yellow-100 text-yellow-800";
    case "High":
      return "bg-orange-100 text-orange-800";
    case "Critical":
      return "bg-red-100 text-red-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
};

export const getRiskChartColor = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case "Low":
      return "#16a34a";
    case "Medium":
      return "#ca8a04";
    case "High":
      return "#ea580c";
    case "Critical":
      return "#dc2626";
    default:
      return "#6b7280";
  }
};

// Score to risk level conversion
export const scoreToRiskLevel = (score: number): RiskLevel => {
  if (score < 0.25) return "Low";
  if (score < 0.50) return "Medium";
  if (score < 0.75) return "High";
  return "Critical";
};

// Format percentage
export const formatPercentage = (value: number): string => {
  return `${value.toFixed(1)}%`;
};

// Format score (0-1 range to percentage)
export const formatScore = (score: number): string => {
  return `${(score * 100).toFixed(1)}%`;
};

// Format date
export const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
};

// Format relative time
export const formatRelativeTime = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInMs = now.getTime() - date.getTime();
  const diffInMinutes = Math.floor(diffInMs / 60000);
  const diffInHours = Math.floor(diffInMs / 3600000);
  const diffInDays = Math.floor(diffInMs / 86400000);

  if (diffInMinutes < 1) return "just now";
  if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
  if (diffInHours < 24) return `${diffInHours}h ago`;
  if (diffInDays < 7) return `${diffInDays}d ago`;
  return formatDate(dateString);
};

// Validate financial health (0-100)
export const validateFinancialHealth = (value: number): boolean => {
  return value >= 0 && value <= 100;
};

// Validate SLA uptime (0-100)
export const validateSlaUptime = (value: number): boolean => {
  return value >= 0 && value <= 100;
};

// Validate major incidents (>= 0)
export const validateMajorIncidents = (value: number): boolean => {
  return value >= 0 && Number.isInteger(value);
};

// Get health status color
export const getHealthColor = (health: number): string => {
  if (health >= 80) return "text-green-600";
  if (health >= 50) return "text-yellow-600";
  return "text-red-600";
};

// Get SLA status color
export const getSlaColor = (sla: number): string => {
  if (sla >= 99) return "text-green-600";
  if (sla >= 95) return "text-yellow-600";
  return "text-red-600";
};
