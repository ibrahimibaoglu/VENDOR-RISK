# Vendor Risk Scoring Engine - Frontend

Modern and responsive Next.js web application for vendor risk assessment and management.

## Features

- **Dashboard**: Overview of vendor statistics, risk distribution, and recent vendors
- **Vendor List**: Paginated vendor listing with risk indicators and filtering
- **Vendor Details**: Comprehensive vendor profile with risk assessment breakdown
- **Add Vendor**: Form to create new vendor profiles with validation
- **Risk Assessment**: Visual representation of financial, operational, and security compliance risks
- **Responsive Design**: Mobile-friendly interface with Tailwind CSS

## Tech Stack

- **Framework**: Next.js 16.1.1 (App Router)
- **Language**: TypeScript 5
- **Styling**: Tailwind CSS 4
- **UI Components**: Custom components with lucide-react icons
- **HTTP Client**: Axios
- **Charts**: Recharts (for future enhancements)

## Prerequisites

- Node.js 20.x or higher
- npm or yarn
- Backend API running on `http://localhost:5001`

## Installation

1. Install dependencies:
```bash
npm install
```

2. Configure environment variables:
```bash
cp .env.local.example .env.local
```

Edit `.env.local` if your API runs on a different URL:
```env
NEXT_PUBLIC_API_URL=http://localhost:5001/api
```

## Development

Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:3000`

## Build

Create a production build:
```bash
npm run build
```

Start the production server:
```bash
npm start
```

## Project Structure

```
frontend/
├── app/                          # Next.js App Router pages
│   ├── layout.tsx               # Root layout with navigation
│   ├── page.tsx                 # Dashboard page
│   └── vendors/
│       ├── page.tsx             # Vendor list page
│       ├── new/
│       │   └── page.tsx         # Add vendor form
│       └── [id]/
│           └── page.tsx         # Vendor detail page
├── components/                   # React components
│   ├── ui/                      # Reusable UI components
│   │   ├── Button.tsx
│   │   ├── Card.tsx
│   │   ├── Badge.tsx
│   │   ├── Input.tsx
│   │   └── Loading.tsx
│   ├── Navigation.tsx           # Main navigation bar
│   └── RiskBadge.tsx           # Risk level badge component
├── lib/                         # Utilities and configurations
│   ├── api.ts                  # API service layer
│   ├── types.ts                # TypeScript type definitions
│   └── utils.ts                # Utility functions
├── public/                      # Static assets
└── package.json                 # Dependencies and scripts
```

## API Integration

The frontend communicates with the backend API through the following endpoints:

### Vendor Management
- `GET /api/vendor?page={page}&pageSize={pageSize}` - Get paginated vendor list
- `GET /api/vendor/{id}` - Get vendor by ID
- `POST /api/vendor` - Create new vendor
- `DELETE /api/vendor/{id}` - Delete vendor

### Risk Assessment
- `GET /api/vendor/{id}/risk` - Get risk assessment for vendor

### Health Check
- `GET /api/health` - API health status

## Pages

### Dashboard (`/`)
- Total vendor count
- High-risk vendor count
- Average financial health and SLA uptime
- Risk distribution chart
- Recent vendors list

### Vendor List (`/vendors`)
- Paginated vendor listing
- Risk badges for each vendor
- Financial health, SLA uptime, and incident metrics
- Security certifications
- Document validation status
- Delete functionality

### Vendor Detail (`/vendors/{id}`)
- Complete vendor profile
- Risk assessment breakdown (financial, operational, security)
- Risk explanation
- Security certifications
- Document validation status
- Financial health and operational performance metrics

### Add Vendor (`/vendors/new`)
- Form with validation
- Basic information (vendor name)
- Financial and operational metrics
- Security certifications (dynamic list)
- Document validation checkboxes
- Real-time validation

## Components

### UI Components
- **Button**: Primary, secondary, danger, and ghost variants
- **Card**: Container with header, title, description, content, and footer
- **Badge**: Color-coded badges for different states
- **Input**: Text input with label and error message support
- **Loading**: Loading spinner with different sizes

### Feature Components
- **Navigation**: Top navigation bar with active link highlighting
- **RiskBadge**: Risk level indicator with color coding (Low, Medium, High, Critical)

## Type Definitions

TypeScript interfaces are defined in `lib/types.ts`:
- `Vendor`: Vendor entity
- `CreateVendorRequest`: Vendor creation payload
- `RiskAssessment`: Risk assessment entity
- `DocumentValidation`: Document validation status
- `RiskLevel`: Risk level enum
- `PaginatedResponse`: Paginated API response

## Utilities

Helper functions in `lib/utils.ts`:
- `getRiskColor()`: Get color class for risk level
- `getRiskBadgeColor()`: Get badge color for risk level
- `formatPercentage()`: Format number as percentage
- `formatScore()`: Format risk score as percentage
- `formatDate()`: Format ISO date string
- `formatRelativeTime()`: Format date as relative time
- `validateFinancialHealth()`: Validate financial health value
- `validateSlaUptime()`: Validate SLA uptime value
- `validateMajorIncidents()`: Validate major incidents value

## Styling

The application uses Tailwind CSS with a custom color palette:
- **Risk Colors**:
  - Low: Green
  - Medium: Yellow
  - High: Orange
  - Critical: Red
- **Brand Colors**: Blue for primary actions
- **Neutral Colors**: Gray scale for backgrounds and text

## Error Handling

- API errors are caught and displayed to users
- Form validation with inline error messages
- Loading states for async operations
- 404 handling for vendor not found

## Future Enhancements

- Search and filter functionality
- Export vendor data to CSV/PDF
- Vendor comparison feature
- Risk trend charts using Recharts
- Dark mode support
- Real-time updates with WebSocket
- Bulk vendor import
- Advanced filtering and sorting

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

This project is part of the Vendor Risk Scoring Engine case study.
