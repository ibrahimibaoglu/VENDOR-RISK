import axios, { AxiosError } from "axios";
import type {
  Vendor,
  CreateVendorRequest,
  RiskAssessment,
  PaginatedResponse,
  HealthStatus,
  ValidationError,
} from "./types";

// API Base URL - can be configured via environment variable
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Error handler
export const handleApiError = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ValidationError>;

    if (axiosError.response?.data) {
      const data = axiosError.response.data;

      // Handle validation errors
      if (data.errors) {
        const errorMessages = Object.entries(data.errors)
          .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
          .join("; ");
        return errorMessages;
      }

      // Handle detail message
      if (data.detail) {
        return data.detail;
      }

      // Handle title
      if (data.title) {
        return data.title;
      }
    }

    return axiosError.message;
  }

  return "An unexpected error occurred";
};

// Vendor API
export const vendorApi = {
  // Get all vendors with pagination
  async getAll(page = 1, pageSize = 10): Promise<PaginatedResponse<Vendor>> {
    const response = await api.get<Vendor[]>("/vendor", {
      params: { page, pageSize },
    });

    return {
      data: response.data,
      pagination: {
        totalCount: parseInt(response.headers["x-total-count"] || "0"),
        page: parseInt(response.headers["x-page"] || String(page)),
        pageSize: parseInt(response.headers["x-page-size"] || String(pageSize)),
      },
    };
  },

  // Get vendor by ID
  async getById(id: number): Promise<Vendor> {
    const response = await api.get<Vendor>(`/vendor/${id}`);
    return response.data;
  },

  // Create new vendor
  async create(vendor: CreateVendorRequest): Promise<Vendor> {
    const response = await api.post<Vendor>("/vendor", vendor);
    return response.data;
  },

  // Delete vendor
  async delete(id: number): Promise<void> {
    await api.delete(`/vendor/${id}`);
  },

  // Get risk assessment for vendor
  async getRiskAssessment(id: number): Promise<RiskAssessment> {
    const response = await api.get<RiskAssessment>(`/vendor/${id}/risk`);
    return response.data;
  },
};

// Health API
export const healthApi = {
  // Basic health check
  async check(): Promise<HealthStatus> {
    const response = await api.get<HealthStatus>("/health");
    return response.data;
  },
};

export default api;
