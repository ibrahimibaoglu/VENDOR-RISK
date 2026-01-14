// API Response Types

export interface DocumentValidation {
  contractValid: boolean;
  privacyPolicyValid: boolean;
  pentestReportValid: boolean;
}

export interface Vendor {
  id: number;
  name: string;
  financialHealth: number;
  slaUptime: number;
  majorIncidents: number;
  securityCerts: string[];
  documents: DocumentValidation;
  createdAt: string;
  updatedAt: string;
}

export interface CreateVendorRequest {
  name: string;
  financialHealth: number;
  slaUptime: number;
  majorIncidents: number;
  securityCerts?: string[];
  documents: DocumentValidation;
}

export interface RiskAssessment {
  id: number;
  vendorId: number;
  financialRiskScore: number;
  operationalRiskScore: number;
  securityComplianceRiskScore: number;
  finalRiskScore: number;
  riskLevel: RiskLevel;
  explanation: string;
  assessedAt: string;
}

export type RiskLevel = "Low" | "Medium" | "High" | "Critical";

export interface PaginationHeaders {
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationHeaders;
}

export interface HealthStatus {
  status: "Healthy" | "Unhealthy";
  timestamp: string;
  version?: string;
}

export interface ValidationError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: {
    [field: string]: string[];
  };
}
