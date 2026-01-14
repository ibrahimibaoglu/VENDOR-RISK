# API Usage Examples

This guide provides practical examples of using the Vendor Risk Scoring API.

## Base URL

```
Development: http://localhost:5001
Docker: http://localhost:5001
Production: https://your-domain.com
```

## Authentication

Currently, the API is **unauthenticated**. For production, add JWT/OAuth authentication.

---

## 1. Create a Vendor

### Request
```http
POST /api/vendor
Content-Type: application/json

{
  "name": "TechPlus Solutions",
  "financialHealth": 78,
  "slaUptime": 93.0,
  "majorIncidents": 1,
  "securityCerts": ["ISO27001"],
  "documents": {
    "contractValid": true,
    "privacyPolicyValid": false,
    "pentestReportValid": true
  }
}
```

### Response (201 Created)
```json
{
  "id": 1,
  "name": "TechPlus Solutions",
  "financialHealth": 78,
  "slaUptime": 93.0,
  "majorIncidents": 1,
  "securityCerts": ["ISO27001"],
  "documents": {
    "contractValid": true,
    "privacyPolicyValid": false,
    "pentestReportValid": true
  },
  "createdAt": "2026-01-14T10:00:00Z",
  "updatedAt": "2026-01-14T10:00:00Z"
}
```

### cURL Example
```bash
curl -X POST http://localhost:5001/api/vendor \
  -H "Content-Type: application/json" \
  -d '{
    "name": "TechPlus Solutions",
    "financialHealth": 78,
    "slaUptime": 93.0,
    "majorIncidents": 1,
    "securityCerts": ["ISO27001"],
    "documents": {
      "contractValid": true,
      "privacyPolicyValid": false,
      "pentestReportValid": true
    }
  }'
```

---

## 2. Get Vendor by ID

### Request
```http
GET /api/vendor/1
```

### Response (200 OK)
```json
{
  "id": 1,
  "name": "TechPlus Solutions",
  "financialHealth": 78,
  "slaUptime": 93.0,
  "majorIncidents": 1,
  "securityCerts": ["ISO27001"],
  "documents": {
    "contractValid": true,
    "privacyPolicyValid": false,
    "pentestReportValid": true
  },
  "createdAt": "2026-01-14T10:00:00Z",
  "updatedAt": "2026-01-14T10:00:00Z"
}
```

### cURL Example
```bash
curl http://localhost:5001/api/vendor/1
```

---

## 3. Get All Vendors (Paginated)

### Request
```http
GET /api/vendor?page=1&pageSize=10
```

### Response Headers
```
X-Total-Count: 15
X-Page: 1
X-Page-Size: 10
```

### Response (200 OK)
```json
[
  {
    "id": 1,
    "name": "TechPlus Solutions",
    "financialHealth": 78,
    "slaUptime": 93.0,
    "majorIncidents": 1,
    "securityCerts": ["ISO27001"],
    "documents": {
      "contractValid": true,
      "privacyPolicyValid": false,
      "pentestReportValid": true
    },
    "createdAt": "2026-01-14T10:00:00Z",
    "updatedAt": "2026-01-14T10:00:00Z"
  },
  // ... more vendors
]
```

### cURL Example
```bash
curl "http://localhost:5001/api/vendor?page=1&pageSize=10"
```

---

## 4. Calculate Risk Assessment

### Request
```http
GET /api/vendor/1/risk
```

### Response (200 OK)
```json
{
  "id": 1,
  "vendorId": 1,
  "financialRiskScore": 0.25,
  "operationalRiskScore": 0.45,
  "securityComplianceRiskScore": 0.65,
  "finalRiskScore": 0.42,
  "riskLevel": "Medium",
  "explanation": "SLA uptime below 95% (93.00%) + 1 major incident(s) in last 12 months + Invalid/expired documents: Privacy Policy",
  "assessedAt": "2026-01-14T10:05:00Z"
}
```

### Risk Breakdown

**Financial Risk (0.25):**
- Financial Health: 78 → Low-Medium risk (0.25)
- Weight: 40% → Contributes 0.10 to final score

**Operational Risk (0.45):**
- SLA Uptime: 93% → Below 95% penalty (+0.35)
- Major Incidents: 1 → Minor penalty (+0.10)
- Total: 0.45
- Weight: 30% → Contributes 0.135 to final score

**Security/Compliance Risk (0.65):**
- Missing ISO27001: No (has it)
- Invalid Privacy Policy: Yes (+0.10)
- Missing Pentest Report: No
- Total: 0.65
- Weight: 30% → Contributes 0.195 to final score

**Final Score:** 0.10 + 0.135 + 0.195 = 0.42 (Medium Risk)

### cURL Example
```bash
curl http://localhost:5001/api/vendor/1/risk
```

---

## 5. Delete Vendor

### Request
```http
DELETE /api/vendor/1
```

### Response (204 No Content)
```
(No body)
```

### cURL Example
```bash
curl -X DELETE http://localhost:5001/api/vendor/1
```

---

## 6. Health Check

### Request
```http
GET /health
```

### Response (200 OK)
```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy",
      "description": "Database connection is healthy"
    },
    "redis": {
      "status": "Healthy",
      "description": "Redis connection is healthy"
    }
  },
  "totalDuration": "00:00:00.0234567"
}
```

### cURL Example
```bash
curl http://localhost:5001/health
```

---

## Real-World Scenarios

### Scenario 1: High-Risk Vendor

```json
{
  "name": "RiskyVendor Inc",
  "financialHealth": 45,
  "slaUptime": 88.0,
  "majorIncidents": 4,
  "securityCerts": [],
  "documents": {
    "contractValid": false,
    "privacyPolicyValid": false,
    "pentestReportValid": false
  }
}
```

**Expected Risk Assessment:**
- Financial Risk: 0.80 (very low financial health)
- Operational Risk: 0.90 (poor SLA + many incidents)
- Security Risk: 0.90 (no certs + all invalid docs)
- **Final Score: ~0.87 (Critical Risk)**

### Scenario 2: Low-Risk Vendor

```json
{
  "name": "SecureVendor Corp",
  "financialHealth": 95,
  "slaUptime": 99.5,
  "majorIncidents": 0,
  "securityCerts": ["ISO27001", "SOC2", "PCI-DSS"],
  "documents": {
    "contractValid": true,
    "privacyPolicyValid": true,
    "pentestReportValid": true
  }
}
```

**Expected Risk Assessment:**
- Financial Risk: 0.10 (excellent financial health)
- Operational Risk: 0.05 (excellent SLA + no incidents)
- Security Risk: 0.05 (all certifications + valid docs)
- **Final Score: ~0.07 (Low Risk)**

---

## Error Responses

### 400 Bad Request (Validation Error)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FinancialHealth": [
      "Financial Health must be between 0 and 100."
    ]
  }
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Vendor with ID 999 not found"
}
```

### 500 Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred"
}
```

---

## JavaScript/TypeScript Examples

### Using Fetch API

```javascript
// Create vendor
async function createVendor() {
  const response = await fetch('http://localhost:5001/api/vendor', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      name: 'TechPlus Solutions',
      financialHealth: 78,
      slaUptime: 93.0,
      majorIncidents: 1,
      securityCerts: ['ISO27001'],
      documents: {
        contractValid: true,
        privacyPolicyValid: false,
        pentestReportValid: true
      }
    })
  });
  
  const vendor = await response.json();
  console.log('Created vendor:', vendor);
  return vendor;
}

// Get risk assessment
async function getRiskAssessment(vendorId) {
  const response = await fetch(`http://localhost:5001/api/vendor/${vendorId}/risk`);
  const assessment = await response.json();
  console.log('Risk assessment:', assessment);
  return assessment;
}
```

### Using Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5001/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Create vendor
const createVendor = async (vendorData) => {
  const response = await api.post('/vendor', vendorData);
  return response.data;
};

// Get all vendors
const getVendors = async (page = 1, pageSize = 10) => {
  const response = await api.get('/vendor', {
    params: { page, pageSize }
  });
  return {
    data: response.data,
    total: parseInt(response.headers['x-total-count'])
  };
};

// Calculate risk
const calculateRisk = async (vendorId) => {
  const response = await api.get(`/vendor/${vendorId}/risk`);
  return response.data;
};
```

---

## Python Examples

```python
import requests

BASE_URL = "http://localhost:5001/api"

# Create vendor
def create_vendor(vendor_data):
    response = requests.post(
        f"{BASE_URL}/vendor",
        json=vendor_data
    )
    return response.json()

# Get vendor
def get_vendor(vendor_id):
    response = requests.get(f"{BASE_URL}/vendor/{vendor_id}")
    return response.json()

# Calculate risk
def calculate_risk(vendor_id):
    response = requests.get(f"{BASE_URL}/vendor/{vendor_id}/risk")
    return response.json()

# Example usage
vendor = create_vendor({
    "name": "TechPlus Solutions",
    "financialHealth": 78,
    "slaUptime": 93.0,
    "majorIncidents": 1,
    "securityCerts": ["ISO27001"],
    "documents": {
        "contractValid": True,
        "privacyPolicyValid": False,
        "pentestReportValid": True
    }
})

risk = calculate_risk(vendor['id'])
print(f"Risk Level: {risk['riskLevel']}")
print(f"Final Score: {risk['finalRiskScore']}")
```

---

## C# Examples

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };

// Create vendor
var vendor = new CreateVendorRequest
{
    Name = "TechPlus Solutions",
    FinancialHealth = 78,
    SlaUptime = 93.0m,
    MajorIncidents = 1,
    SecurityCerts = new List<string> { "ISO27001" },
    Documents = new DocumentValidationDto
    {
        ContractValid = true,
        PrivacyPolicyValid = false,
        PentestReportValid = true
    }
};

var response = await client.PostAsJsonAsync("/api/vendor", vendor);
var createdVendor = await response.Content.ReadFromJsonAsync<VendorResponse>();

// Get risk assessment
var riskResponse = await client.GetAsync($"/api/vendor/{createdVendor.Id}/risk");
var assessment = await riskResponse.Content.ReadFromJsonAsync<RiskAssessmentResponse>();

Console.WriteLine($"Risk Level: {assessment.RiskLevel}");
Console.WriteLine($"Final Score: {assessment.FinalRiskScore}");
```

---

## Postman Collection

Import this collection into Postman for quick testing:

```json
{
  "info": {
    "name": "Vendor Risk API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Create Vendor",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/vendor",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"name\": \"TechPlus Solutions\",\n  \"financialHealth\": 78,\n  \"slaUptime\": 93.0,\n  \"majorIncidents\": 1,\n  \"securityCerts\": [\"ISO27001\"],\n  \"documents\": {\n    \"contractValid\": true,\n    \"privacyPolicyValid\": false,\n    \"pentestReportValid\": true\n  }\n}"
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5001"
    }
  ]
}
```

---

For more examples, visit the **Swagger UI** at http://localhost:5001
