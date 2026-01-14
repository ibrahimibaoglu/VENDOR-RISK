"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import Link from "next/link";
import { vendorApi, handleApiError } from "@/lib/api";
import type { Vendor, RiskAssessment } from "@/lib/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import Button from "@/components/ui/Button";
import Loading from "@/components/ui/Loading";
import RiskBadge from "@/components/RiskBadge";
import {
  ArrowLeft,
  Shield,
  AlertTriangle,
  FileText,
  Clock,
  TrendingUp,
  Activity,
  CheckCircle,
  XCircle,
} from "lucide-react";
import { formatDate, getHealthColor, getSlaColor } from "@/lib/utils";

export default function VendorDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [vendor, setVendor] = useState<Vendor | null>(null);
  const [risk, setRisk] = useState<RiskAssessment | null>(null);

  useEffect(() => {
    if (id) {
      loadVendor();
    }
  }, [id]);

  const loadVendor = async () => {
    try {
      setLoading(true);
      setError(null);

      const vendorData = await vendorApi.getById(parseInt(id));
      setVendor(vendorData);

      try {
        const riskData = await vendorApi.getRiskAssessment(parseInt(id));
        setRisk(riskData);
      } catch {
        // Risk assessment might not exist
      }
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loading size="lg" />
      </div>
    );
  }

  if (error || !vendor) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">{error || "Vendor not found"}</p>
        </div>
        <Link href="/vendors" className="mt-4 inline-block">
          <Button variant="secondary">Back to Vendors</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <Link href="/vendors">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Vendors
          </Button>
        </Link>
      </div>

      <div className="mb-8">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{vendor.name}</h1>
            <p className="mt-2 text-gray-600">Vendor ID: {vendor.id}</p>
          </div>
          {risk && <RiskBadge riskLevel={risk.riskLevel} score={risk.finalRiskScore} />}
        </div>
      </div>

      {/* Risk Assessment Card */}
      {risk && (
        <Card className="mb-6">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-orange-500" />
              Risk Assessment
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
              <div>
                <p className="text-sm text-gray-500 mb-1">Financial Risk</p>
                <div className="flex items-center gap-2">
                  <div className="flex-1 bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-blue-500 h-2 rounded-full"
                      style={{ width: `${risk.financialRiskScore * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium">
                    {(risk.financialRiskScore * 100).toFixed(1)}%
                  </span>
                </div>
              </div>

              <div>
                <p className="text-sm text-gray-500 mb-1">Operational Risk</p>
                <div className="flex items-center gap-2">
                  <div className="flex-1 bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-yellow-500 h-2 rounded-full"
                      style={{ width: `${risk.operationalRiskScore * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium">
                    {(risk.operationalRiskScore * 100).toFixed(1)}%
                  </span>
                </div>
              </div>

              <div>
                <p className="text-sm text-gray-500 mb-1">Security & Compliance Risk</p>
                <div className="flex items-center gap-2">
                  <div className="flex-1 bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-red-500 h-2 rounded-full"
                      style={{ width: `${risk.securityComplianceRiskScore * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium">
                    {(risk.securityComplianceRiskScore * 100).toFixed(1)}%
                  </span>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 rounded-lg p-4">
              <p className="text-sm font-medium text-gray-700 mb-2">Risk Explanation</p>
              <p className="text-sm text-gray-600">{risk.explanation}</p>
            </div>

            <div className="mt-4 flex items-center text-sm text-gray-500">
              <Clock className="h-4 w-4 mr-1" />
              Assessed {formatDate(risk.assessedAt)}
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        {/* Financial Health */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-green-500" />
              Financial Health
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-center">
              <div className={`text-4xl font-bold ${getHealthColor(vendor.financialHealth)}`}>
                {vendor.financialHealth}
              </div>
              <p className="text-sm text-gray-500 mt-1">out of 100</p>
              <div className="mt-4 w-full bg-gray-200 rounded-full h-3">
                <div
                  className={`h-3 rounded-full ${
                    vendor.financialHealth >= 80
                      ? "bg-green-500"
                      : vendor.financialHealth >= 50
                      ? "bg-yellow-500"
                      : "bg-red-500"
                  }`}
                  style={{ width: `${vendor.financialHealth}%` }}
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Operational Performance */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5 text-blue-500" />
              Operational Performance
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div>
                <div className="flex justify-between mb-1">
                  <span className="text-sm text-gray-600">SLA Uptime</span>
                  <span className={`text-sm font-medium ${getSlaColor(vendor.slaUptime)}`}>
                    {vendor.slaUptime}%
                  </span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full ${
                      vendor.slaUptime >= 99
                        ? "bg-green-500"
                        : vendor.slaUptime >= 95
                        ? "bg-yellow-500"
                        : "bg-red-500"
                    }`}
                    style={{ width: `${vendor.slaUptime}%` }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Major Incidents (Last 12 months)</span>
                  <span
                    className={`text-2xl font-bold ${
                      vendor.majorIncidents === 0
                        ? "text-green-600"
                        : vendor.majorIncidents <= 2
                        ? "text-yellow-600"
                        : "text-red-600"
                    }`}
                  >
                    {vendor.majorIncidents}
                  </span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Security Certifications */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5 text-purple-500" />
              Security Certifications
            </CardTitle>
          </CardHeader>
          <CardContent>
            {vendor.securityCerts.length > 0 ? (
              <div className="space-y-2">
                {vendor.securityCerts.map((cert) => (
                  <div
                    key={cert}
                    className="flex items-center gap-2 bg-blue-50 border border-blue-200 rounded-lg px-4 py-2"
                  >
                    <CheckCircle className="h-4 w-4 text-blue-600" />
                    <span className="font-medium text-blue-900">{cert}</span>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8">
                <XCircle className="h-12 w-12 text-gray-300 mx-auto mb-2" />
                <p className="text-sm text-gray-500">No security certifications</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Document Validation */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-indigo-500" />
              Document Validation
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Contract</span>
                {vendor.documents.contractValid ? (
                  <div className="flex items-center gap-1 text-green-600">
                    <CheckCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Valid</span>
                  </div>
                ) : (
                  <div className="flex items-center gap-1 text-red-600">
                    <XCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Invalid</span>
                  </div>
                )}
              </div>

              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Privacy Policy</span>
                {vendor.documents.privacyPolicyValid ? (
                  <div className="flex items-center gap-1 text-green-600">
                    <CheckCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Valid</span>
                  </div>
                ) : (
                  <div className="flex items-center gap-1 text-red-600">
                    <XCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Invalid</span>
                  </div>
                )}
              </div>

              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Pentest Report</span>
                {vendor.documents.pentestReportValid ? (
                  <div className="flex items-center gap-1 text-green-600">
                    <CheckCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Valid</span>
                  </div>
                ) : (
                  <div className="flex items-center gap-1 text-red-600">
                    <XCircle className="h-4 w-4" />
                    <span className="text-sm font-medium">Invalid</span>
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Metadata */}
      <div className="mt-6 text-sm text-gray-500 text-center">
        Created {formatDate(vendor.createdAt)} • Last updated {formatDate(vendor.updatedAt)}
      </div>
    </div>
  );
}
