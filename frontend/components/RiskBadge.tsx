import type { RiskLevel } from "@/lib/types";
import { getRiskBadgeColor } from "@/lib/utils";
import Badge from "./ui/Badge";

interface RiskBadgeProps {
  riskLevel: RiskLevel;
  score?: number;
}

export default function RiskBadge({ riskLevel, score }: RiskBadgeProps) {
  return (
    <Badge className={getRiskBadgeColor(riskLevel)}>
      {riskLevel}
      {score !== undefined && ` (${(score * 100).toFixed(1)}%)`}
    </Badge>
  );
}
