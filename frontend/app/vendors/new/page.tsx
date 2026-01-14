"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { vendorApi, handleApiError } from "@/lib/api";
import type { CreateVendorRequest } from "@/lib/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import Button from "@/components/ui/Button";
import Input from "@/components/ui/Input";
import { ArrowLeft, Plus, X } from "lucide-react";
import {
  validateFinancialHealth,
  validateSlaUptime,
  validateMajorIncidents,
} from "@/lib/utils";

export default function NewVendorPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<CreateVendorRequest>({
    name: "",
    financialHealth: 0,
    slaUptime: 0,
    majorIncidents: 0,
    securityCerts: [],
    documents: {
      contractValid: false,
      privacyPolicyValid: false,
      pentestReportValid: false,
    },
  });

  const [newCert, setNewCert] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setErrors({});

    // Validate form
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = "Name is required";
    }

    if (!validateFinancialHealth(formData.financialHealth)) {
      newErrors.financialHealth = "Must be between 0 and 100";
    }

    if (!validateSlaUptime(formData.slaUptime)) {
      newErrors.slaUptime = "Must be between 0 and 100";
    }

    if (!validateMajorIncidents(formData.majorIncidents)) {
      newErrors.majorIncidents = "Must be a positive integer";
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    try {
      setLoading(true);
      const vendor = await vendorApi.create(formData);
      router.push(`/vendors/${vendor.id}`);
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleAddCert = () => {
    if (newCert.trim() && !formData.securityCerts?.includes(newCert.trim())) {
      setFormData({
        ...formData,
        securityCerts: [...(formData.securityCerts || []), newCert.trim()],
      });
      setNewCert("");
    }
  };

  const handleRemoveCert = (cert: string) => {
    setFormData({
      ...formData,
      securityCerts: formData.securityCerts?.filter((c) => c !== cert) || [],
    });
  };

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <Link href="/vendors">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Vendors
          </Button>
        </Link>
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Add New Vendor</h1>
        <p className="mt-2 text-gray-600">
          Enter vendor information to assess risk profile
        </p>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <Input
                label="Vendor Name *"
                placeholder="e.g., TechPlus Solutions"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
                error={errors.name}
              />
            </div>
          </CardContent>
        </Card>

        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Financial & Operational Metrics</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Input
                label="Financial Health (0-100) *"
                type="number"
                min="0"
                max="100"
                placeholder="78"
                value={formData.financialHealth || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    financialHealth: parseFloat(e.target.value) || 0,
                  })
                }
                error={errors.financialHealth}
              />

              <Input
                label="SLA Uptime (%) *"
                type="number"
                min="0"
                max="100"
                step="0.1"
                placeholder="99.5"
                value={formData.slaUptime || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    slaUptime: parseFloat(e.target.value) || 0,
                  })
                }
                error={errors.slaUptime}
              />

              <Input
                label="Major Incidents (Last 12 months) *"
                type="number"
                min="0"
                placeholder="0"
                value={formData.majorIncidents || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    majorIncidents: parseInt(e.target.value) || 0,
                  })
                }
                error={errors.majorIncidents}
              />
            </div>
          </CardContent>
        </Card>

        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Security Certifications</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex gap-2">
                <Input
                  placeholder="e.g., ISO27001, SOC2, PCI-DSS"
                  value={newCert}
                  onChange={(e) => setNewCert(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      handleAddCert();
                    }
                  }}
                />
                <Button
                  type="button"
                  onClick={handleAddCert}
                  variant="secondary"
                  className="flex-shrink-0"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add
                </Button>
              </div>

              {formData.securityCerts && formData.securityCerts.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {formData.securityCerts.map((cert) => (
                    <span
                      key={cert}
                      className="inline-flex items-center gap-1 px-3 py-1.5 bg-blue-100 text-blue-800 rounded-full text-sm font-medium"
                    >
                      {cert}
                      <button
                        type="button"
                        onClick={() => handleRemoveCert(cert)}
                        className="ml-1 hover:bg-blue-200 rounded-full p-0.5"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Document Validation</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <label className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg cursor-pointer hover:bg-gray-100 transition-colors">
                <input
                  type="checkbox"
                  checked={formData.documents.contractValid}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      documents: {
                        ...formData.documents,
                        contractValid: e.target.checked,
                      },
                    })
                  }
                  className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700">
                  Contract is valid and up-to-date
                </span>
              </label>

              <label className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg cursor-pointer hover:bg-gray-100 transition-colors">
                <input
                  type="checkbox"
                  checked={formData.documents.privacyPolicyValid}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      documents: {
                        ...formData.documents,
                        privacyPolicyValid: e.target.checked,
                      },
                    })
                  }
                  className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700">
                  Privacy Policy is valid and compliant
                </span>
              </label>

              <label className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg cursor-pointer hover:bg-gray-100 transition-colors">
                <input
                  type="checkbox"
                  checked={formData.documents.pentestReportValid}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      documents: {
                        ...formData.documents,
                        pentestReportValid: e.target.checked,
                      },
                    })
                  }
                  className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700">
                  Penetration Test Report is valid and recent
                </span>
              </label>
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-4">
          <Button type="submit" disabled={loading} className="flex-1">
            {loading ? "Creating..." : "Create Vendor"}
          </Button>
          <Link href="/vendors" className="flex-1">
            <Button type="button" variant="secondary" className="w-full">
              Cancel
            </Button>
          </Link>
        </div>
      </form>
    </div>
  );
}
