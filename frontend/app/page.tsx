"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { vendorApi, handleApiError } from "@/lib/api";
import type { Vendor, RiskAssessment } from "@/lib/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import Loading from "@/components/ui/Loading";
import RiskBadge from "@/components/RiskBadge";
import { AlertTriangle, Shield, TrendingUp, Users } from "lucide-react";
import { formatRelativeTime } from "@/lib/utils";

interface DashboardStats {
  totalVendors: number;
  lowRisk: number;
  mediumRisk: number;
  highRisk: number;
  criticalRisk: number;
  averageFinancialHealth: number;
  averageSlaUptime: number;
}

interface VendorWithRisk extends Vendor {
  risk?: RiskAssessment;
}

export default function Dashboard() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<DashboardStats>({
    totalVendors: 0,
    lowRisk: 0,
    mediumRisk: 0,
    highRisk: 0,
    criticalRisk: 0,
    averageFinancialHealth: 0,
    averageSlaUptime: 0,
  });
  const [recentVendors, setRecentVendors] = useState<VendorWithRisk[]>([]);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch all vendors
      const response = await vendorApi.getAll(1, 100);
      const vendors = response.data;

      // Fetch risk assessments for all vendors
      const vendorsWithRisk: VendorWithRisk[] = await Promise.all(
        vendors.map(async (vendor) => {
          try {
            const risk = await vendorApi.getRiskAssessment(vendor.id);
            return { ...vendor, risk };
          } catch {
            return vendor;
          }
        })
      );

      // Calculate statistics
      const riskCounts = {
        Low: 0,
        Medium: 0,
        High: 0,
        Critical: 0,
      };

      vendorsWithRisk.forEach((vendor) => {
        if (vendor.risk) {
          riskCounts[vendor.risk.riskLevel]++;
        }
      });

      const avgFinancial =
        vendors.reduce((sum, v) => sum + v.financialHealth, 0) / vendors.length || 0;
      const avgSla =
        vendors.reduce((sum, v) => sum + v.slaUptime, 0) / vendors.length || 0;

      setStats({
        totalVendors: vendors.length,
        lowRisk: riskCounts.Low,
        mediumRisk: riskCounts.Medium,
        highRisk: riskCounts.High,
        criticalRisk: riskCounts.Critical,
        averageFinancialHealth: avgFinancial,
        averageSlaUptime: avgSla,
      });

      // Get 5 most recent vendors
      setRecentVendors(
        vendorsWithRisk
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 5)
      );
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

  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">Dashboard</h1>
        <p className="mt-2 text-muted-foreground">
          Overview of vendor risk assessments and statistics
        </p>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Vendors
            </CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalVendors}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              High Risk
            </CardTitle>
            <AlertTriangle className="h-4 w-4 text-destructive" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-destructive">
              {stats.highRisk + stats.criticalRisk}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.highRisk} High, {stats.criticalRisk} Critical
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Avg Financial Health
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-emerald-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {stats.averageFinancialHealth.toFixed(1)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">Out of 100</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Avg SLA Uptime
            </CardTitle>
            <Shield className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {stats.averageSlaUptime.toFixed(1)}%
            </div>
            <p className="text-xs text-muted-foreground mt-1">Average uptime</p>
          </CardContent>
        </Card>
      </div>

      {/* Risk Distribution */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <Card>
          <CardHeader>
            <CardTitle>Risk Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div>
                <div className="flex justify-between mb-1">
                  <span className="text-sm font-medium text-emerald-600 dark:text-emerald-500">Low Risk</span>
                  <span className="text-sm text-muted-foreground">{stats.lowRisk}</span>
                </div>
                <div className="w-full bg-secondary rounded-full h-2">
                  <div
                    className="bg-emerald-500 h-2 rounded-full"
                    style={{
                      width: `${(stats.lowRisk / stats.totalVendors) * 100}%`,
                    }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="text-sm font-medium text-amber-600 dark:text-amber-500">Medium Risk</span>
                  <span className="text-sm text-muted-foreground">{stats.mediumRisk}</span>
                </div>
                <div className="w-full bg-secondary rounded-full h-2">
                  <div
                    className="bg-amber-500 h-2 rounded-full"
                    style={{
                      width: `${(stats.mediumRisk / stats.totalVendors) * 100}%`,
                    }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="text-sm font-medium text-orange-600 dark:text-orange-500">High Risk</span>
                  <span className="text-sm text-muted-foreground">{stats.highRisk}</span>
                </div>
                <div className="w-full bg-secondary rounded-full h-2">
                  <div
                    className="bg-orange-500 h-2 rounded-full"
                    style={{
                      width: `${(stats.highRisk / stats.totalVendors) * 100}%`,
                    }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="text-sm font-medium text-destructive">Critical Risk</span>
                  <span className="text-sm text-muted-foreground">{stats.criticalRisk}</span>
                </div>
                <div className="w-full bg-secondary rounded-full h-2">
                  <div
                    className="bg-destructive h-2 rounded-full"
                    style={{
                      width: `${(stats.criticalRisk / stats.totalVendors) * 100}%`,
                    }}
                  />
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Recent Vendors */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Vendors</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentVendors.length === 0 ? (
                <p className="text-sm text-muted-foreground">No vendors yet</p>
              ) : (
                recentVendors.map((vendor) => (
                  <Link
                    key={vendor.id}
                    href={`/vendors/${vendor.id}`}
                    className="block hover:bg-muted/50 p-3 -mx-2 rounded-lg transition-colors group"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-foreground truncate group-hover:text-primary transition-colors">
                          {vendor.name}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {formatRelativeTime(vendor.createdAt)}
                        </p>
                      </div>
                      {vendor.risk && (
                        <RiskBadge riskLevel={vendor.risk.riskLevel} />
                      )}
                    </div>
                  </Link>
                ))
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
