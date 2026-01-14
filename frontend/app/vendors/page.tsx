"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { vendorApi, handleApiError } from "@/lib/api";
import type { Vendor, RiskAssessment } from "@/lib/types";
import { Card, CardContent } from "@/components/ui/Card";
import Button from "@/components/ui/Button";
import Loading from "@/components/ui/Loading";
import RiskBadge from "@/components/RiskBadge";
import {
  ChevronLeft,
  ChevronRight,
  Eye,
  Trash2,
  Shield,
  FileText,
  AlertCircle,
} from "lucide-react";
import { formatRelativeTime, getHealthColor, getSlaColor } from "@/lib/utils";

interface VendorWithRisk extends Vendor {
  risk?: RiskAssessment;
}

export default function VendorsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [vendors, setVendors] = useState<VendorWithRisk[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(10);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  useEffect(() => {
    loadVendors();
  }, [page]);

  const loadVendors = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await vendorApi.getAll(page, pageSize);
      setTotalCount(response.pagination.totalCount);

      // Fetch risk assessments for each vendor
      const vendorsWithRisk: VendorWithRisk[] = await Promise.all(
        response.data.map(async (vendor) => {
          try {
            const risk = await vendorApi.getRiskAssessment(vendor.id);
            return { ...vendor, risk };
          } catch {
            return vendor;
          }
        })
      );

      setVendors(vendorsWithRisk);
    } catch (err) {
      setError(handleApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Are you sure you want to delete this vendor?")) {
      return;
    }

    try {
      setDeletingId(id);
      await vendorApi.delete(id);
      await loadVendors();
    } catch (err) {
      alert(handleApiError(err));
    } finally {
      setDeletingId(null);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  if (loading && vendors.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loading size="lg" />
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Vendors</h1>
          <p className="mt-2 text-gray-600">
            Manage and assess vendor risk profiles
          </p>
        </div>
        <Link href="/vendors/new">
          <Button>Add Vendor</Button>
        </Link>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      {vendors.length === 0 && !loading ? (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-gray-500">No vendors found. Add your first vendor to get started.</p>
            <Link href="/vendors/new" className="mt-4 inline-block">
              <Button>Add Vendor</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="space-y-4 mb-6">
            {vendors.map((vendor) => (
              <Card key={vendor.id} className="hover:shadow-md transition-shadow">
                <CardContent className="p-6">
                  <div className="flex items-start justify-between">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-semibold text-gray-900">
                          {vendor.name}
                        </h3>
                        {vendor.risk && (
                          <RiskBadge
                            riskLevel={vendor.risk.riskLevel}
                            score={vendor.risk.finalRiskScore}
                          />
                        )}
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                        <div className="flex items-center gap-2">
                          <Shield className="h-4 w-4 text-gray-400" />
                          <div>
                            <p className="text-xs text-gray-500">Financial Health</p>
                            <p className={`text-sm font-medium ${getHealthColor(vendor.financialHealth)}`}>
                              {vendor.financialHealth}/100
                            </p>
                          </div>
                        </div>

                        <div className="flex items-center gap-2">
                          <AlertCircle className="h-4 w-4 text-gray-400" />
                          <div>
                            <p className="text-xs text-gray-500">SLA Uptime</p>
                            <p className={`text-sm font-medium ${getSlaColor(vendor.slaUptime)}`}>
                              {vendor.slaUptime}%
                            </p>
                          </div>
                        </div>

                        <div className="flex items-center gap-2">
                          <FileText className="h-4 w-4 text-gray-400" />
                          <div>
                            <p className="text-xs text-gray-500">Major Incidents</p>
                            <p className={`text-sm font-medium ${vendor.majorIncidents > 0 ? 'text-red-600' : 'text-green-600'}`}>
                              {vendor.majorIncidents}
                            </p>
                          </div>
                        </div>
                      </div>

                      <div className="mt-4 flex flex-wrap gap-2">
                        {vendor.securityCerts.length > 0 ? (
                          vendor.securityCerts.map((cert) => (
                            <span
                              key={cert}
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                            >
                              {cert}
                            </span>
                          ))
                        ) : (
                          <span className="text-xs text-gray-500">No certifications</span>
                        )}
                      </div>

                      <div className="mt-2 flex items-center gap-4 text-xs text-gray-500">
                        <span>Created {formatRelativeTime(vendor.createdAt)}</span>
                        <span>•</span>
                        <span>
                          Documents:{" "}
                          <span className={vendor.documents.contractValid ? "text-green-600" : "text-red-600"}>
                            Contract
                          </span>
                          ,{" "}
                          <span className={vendor.documents.privacyPolicyValid ? "text-green-600" : "text-red-600"}>
                            Privacy
                          </span>
                          ,{" "}
                          <span className={vendor.documents.pentestReportValid ? "text-green-600" : "text-red-600"}>
                            Pentest
                          </span>
                        </span>
                      </div>
                    </div>

                    <div className="flex gap-2 ml-4">
                      <Link href={`/vendors/${vendor.id}`}>
                        <Button variant="ghost" size="sm">
                          <Eye className="h-4 w-4" />
                        </Button>
                      </Link>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDelete(vendor.id)}
                        disabled={deletingId === vendor.id}
                      >
                        {deletingId === vendor.id ? (
                          <Loading size="sm" />
                        ) : (
                          <Trash2 className="h-4 w-4" />
                        )}
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6 rounded-lg">
              <div className="flex flex-1 justify-between sm:hidden">
                <Button
                  variant="secondary"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
              <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm text-gray-700">
                    Showing <span className="font-medium">{(page - 1) * pageSize + 1}</span> to{" "}
                    <span className="font-medium">
                      {Math.min(page * pageSize, totalCount)}
                    </span>{" "}
                    of <span className="font-medium">{totalCount}</span> vendors
                  </p>
                </div>
                <div>
                  <nav className="inline-flex -space-x-px rounded-md shadow-sm">
                    <Button
                      variant="secondary"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                      className="rounded-r-none"
                    >
                      <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <span className="inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-700">
                      {page} / {totalPages}
                    </span>
                    <Button
                      variant="secondary"
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page === totalPages}
                      className="rounded-l-none"
                    >
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </nav>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
