# GitHub Copilot Instructions for PaceLab

## Project Overview

PaceLab is a fitness analytics web application that provides effort-adjusted performance metrics for cyclists and runners. The core value proposition: **"Raw pace lies. Effort Doesn't."** The app syncs activities from Strava, fetches historical weather data, and calculates adjusted performance metrics that account for environmental conditions (headwind, heat, cold, rain, elevation).

**Target Users:** Serious cyclists and runners who train with purpose and want to understand their true effort level regardless of conditions.

---

## Technology Stack

### Backend
- **Framework:** ASP.NET Core 8 Web API (C#)
- **Database:** PostgreSQL (hosted on Neon)
- **ORM:** Entity Framework Core with Npgsql
- **Authentication:** JWT Bearer tokens
- **External APIs:** Strava API v3, Weather API (OpenWeather or similar)

### Frontend
- **Framework:** Next.js 14 (App Router) + TypeScript
- **Styling:** Tailwind CSS
- **UI Components:** shadcn/ui

### Infrastructure
- **Database:** Neon (PostgreSQL, AWS US East Ohio)
- **Backend Hosting:** Railway, Render, or Azure
- **Frontend Hosting:** Vercel

---

## Architecture Principles

### Backend Architecture (3-Layer)

```
Controllers (Thin)
    ↓
Services (Thick - Business Logic)
    ↓
Data Layer (EF Core DbContext)
```

**Key Rules:**
1. **Controllers** handle HTTP only - validate input, call services, return responses
2. **Services** contain all business logic, orchestration, calculations
3. **No Repository Pattern** - EF Core is already a repository, don't wrap it
4. **Models** for all models; models for db entities. requests, responses, and dto for data.
5. **Interfaces** only for external APIs (Strava, Weather) - skip for internal services unless needed for testing

### Frontend Architecture

```
app/                        # Next.js App Router
├── (marketing)/           # Public pages (SSG)
│   ├── page.tsx          # Landing page
│   ├── features/
│   └── pricing/
│
└── (app)/                 # Dashboard (CSR, auth required)
    ├── dashboard/
    ├── activities/
    └── settings/

components/                # Reusable UI components
lib/                       # API client, utilities
```

**Key Rules:**
1. Marketing pages use Server Components (SSG) for SEO
2. Dashboard uses Client Components, calls C# API
3. All API calls go through a centralized API client (`lib/api.ts`)
4. Use TanStack Query for data fetching, caching, mutations

---

## Database Schema

### Key Tables

#### users
- `id` (UUID, PK)
- `email` (TEXT, unique index)
- `password_hash` (TEXT)
- `strava_user_id` (BIGINT, unique index, nullable)
- `strava_access_token`, `strava_refresh_token` (TEXT, nullable)
- `strava_token_expires_at`, `last_strava_sync` (TIMESTAMP, nullable)
- Settings: `distance_unit`, `temperature_unit`, `speed_format`, `auto_sync_frequency`, `sync_cycling`, `sync_running`
- `created_at`, `updated_at` (TIMESTAMP)

#### activities
- `id` (UUID, PK)
- `user_id` (UUID, FK → users, cascading delete)
- `strava_activity_id` (BIGINT, unique index)
- `activity_type`, `activity_name` (TEXT)
- `start_date` (TIMESTAMP)
- `distance_meters`, `moving_time_seconds`, `elapsed_time_seconds` (REAL/INTEGER)
- `average_speed_mps`, `max_speed_mps` (REAL)
- `total_elevation_gain` (REAL, nullable)
- `start_latitude`, `start_longitude`, `end_latitude`, `end_longitude` (REAL, nullable)
- `polyline` (TEXT, nullable - encoded from Strava)
- `average_heartrate`, `max_heartrate`, `average_watts`, `calories` (INTEGER/REAL, nullable)
- `created_at`, `updated_at`, `synced_at` (TIMESTAMP)

#### activity_weather (1-to-1 with activities)
- `id` (UUID, PK)
- `activity_id` (UUID, FK → activities, unique index, cascading delete)
- `temperature_celsius`, `wind_speed_mps`, `precipitation_mm` (REAL)
- `wind_direction_deg`, `humidity_percent` (INTEGER)
- `weather_condition` (TEXT)
- `fetched_at` (TIMESTAMP)

#### activity_adjustments (1-to-1 with activities)
- `id` (UUID, PK)
- `activity_id` (UUID, FK → activities, unique index, cascading delete)
- `adjusted_speed_mps`, `adjusted_time_seconds` (REAL/INTEGER)
- `headwind_adjustment_mps`, `heat_adjustment_mps`, `cold_adjustment_mps`, `rain_adjustment_mps` (REAL)
- `total_adjustment_mps`, `total_adjustment_pct` (REAL)
- `difficulty_rating` (TEXT - e.g., "Challenging")
- `calculated_at` (TIMESTAMP)

### Data Types
- Use `REAL` (float in C#) for all measurements (distance, speed, coordinates, weather)
- Use `INTEGER` for time durations, counts, percentages
- Use `TEXT` for strings (no VARCHAR limits in PostgreSQL)
- Use `TIMESTAMP` for dates (no timezone, store UTC)
- Use `UUID` for all IDs

---

## Core Business Logic

### Activity Sync Flow
1. User connects Strava via OAuth 2.0
2. Data syncs on login and when the user manually triggers it via UI
3. Call Strava API to get recent activities (last 30 days)
4. For each new activity:
   - Check if `strava_activity_id` already exists (skip if yes)
   - Insert into `activities` table
   - Fetch historical weather data for activity start time/location
   - Insert into `activity_weather` table
   - Calculate effort adjustments based on weather
   - Insert into `activity_adjustments` table

### Controller Patterns
- All endpoints use `[Authorize]` except auth endpoints
- Extract `userId` from JWT: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- Return `ActionResult<T>` for type safety
- Use DTOs (never return database models directly)
- Keep controllers thin - just call service methods

---

## Service Layer Patterns

### Service Structure
```csharp
public class ActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly IStravaService _stravaService;
    private readonly IWeatherService _weatherService;
    private readonly IAdjustmentService _adjustmentService;

    // Constructor with DI

    // Public methods that controllers call
    public async Task<PagedResponse<ActivityResponse>> GetActivities(Guid userId, int page, int limit, string? type = null)
    {
        // Business logic here
    }
}
```

### Service Responsibilities
- Orchestrate multiple operations
- Call EF Core for database access (no repository layer)
- Call external services (Strava, Weather)
- Transform models to DTOs
- Handle business logic and validation
- Manage transactions

### DTO Patterns
**Always use DTOs for API responses:**
```csharp
// Good
public class ActivityResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public float DistanceMeters { get; set; }
    public float AverageSpeedMps { get; set; }
    public float? AdjustedSpeedMps { get; set; }
    
    // Computed properties for frontend
    public float DistanceMiles => DistanceMeters / 1609.34f;
    public float AveragePaceMph => AverageSpeedMps * 2.237f;
}

// Bad - never return models directly
public async Task<Activity> GetActivity(Guid id) { } // ❌
```

---

## External API Integration

### Strava API
**Base URL:** `https://www.strava.com/api/v3`

**Key Endpoints:**
- `GET /athlete/activities` - Get recent activities
- `GET /activities/:id` - Get activity detail
- `POST /oauth/token` - Refresh access token

**Authentication:**
- Use OAuth 2.0 flow
- Store `access_token`, `refresh_token`, `expires_at` in users table
- Refresh token when expired (check before each API call)

**Rate Limits:**
- 100 requests per 15 minutes
- 1000 requests per day
- Implement exponential backoff on 429 errors

### Weather API
**Providers:** OpenWeather, WeatherAPI.com, or similar

**Endpoint:** Historical weather data
**Required data:** Temperature, wind speed, wind direction, humidity, precipitation

**Caching:**
- Weather data doesn't change - fetch once per activity
- Store in `activity_weather` table
- Don't re-fetch unless recalculating

---

## Frontend Patterns

### API Client Structure
```typescript
// lib/api.ts
const API_URL = process.env.NEXT_PUBLIC_API_URL;

export const api = {
  async getActivities(page = 1, limit = 20) {
    const res = await fetch(`${API_URL}/activities?page=${page}&limit=${limit}`, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
    });
    if (!res.ok) throw new Error('Failed to fetch activities');
    return res.json();
  },

  async getActivity(id: string) {
    const res = await fetch(`${API_URL}/activities/${id}`, {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    return res.json();
  }
};
```

### TanStack Query Patterns
```typescript
// hooks/useActivities.ts
export function useActivities(page = 1, limit = 20) {
  return useQuery({
    queryKey: ['activities', page, limit],
    queryFn: () => api.getActivities(page, limit),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// Usage in component
'use client';

export default function ActivitiesPage() {
  const { data, isLoading, error } = useActivities(1, 20);

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage error={error} />;

  return <ActivityList activities={data.data} />;
}
```

### Component Patterns
```typescript
// Use shadcn/ui components
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';

// Tailwind for styling (no separate CSS files)
<div className="flex items-center justify-between p-4 bg-white rounded-lg shadow">

// Server Components for static pages (marketing)
export default async function HomePage() {
  return <LandingPage />;
}

// Client Components for interactive UI (dashboard)
'use client';
export default function Dashboard() {
  const { data } = useDashboard();
  return <DashboardView data={data} />;
}
```

---

## Code Style & Conventions

### C# Conventions
- Use `async/await` for all I/O operations
- Use `var` for local variables when type is obvious
- PascalCase for public members, camelCase for private fields with `_` prefix
- Use nullable reference types (`string?` for nullable)

### TypeScript Conventions
- Use interfaces for data shapes
- Use `async/await` (not `.then()`)
- Prefer `const` over `let`
- Use optional chaining (`?.`) and nullish coalescing (`??`)
- Export named exports (not default) for utilities

### File Naming
- **C#:** PascalCase (e.g., `ActivityService.cs`, `IActivityService.cs`)
- **TypeScript:** kebab-case (e.g., `activity-card.tsx`, `use-activities.ts`)
- **React Components:** PascalCase (e.g., `ActivityCard.tsx`)

---

## Testing Strategy

### Backend Testing
- Use EF Core In-Memory database for service tests
- Integration tests for controllers (WebApplicationFactory)
- Mock external APIs (IStravaService, IWeatherService)

```csharp
// Example service test
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;

var context = new ApplicationDbContext(options);
var service = new ActivityService(context);

// Seed test data
context.Activities.Add(new Activity { /* ... */ });
await context.SaveChangesAsync();

// Test
var result = await service.GetActivities(userId, 1, 20);
Assert.Equal(1, result.Data.Count);
```

### Frontend Testing
- Use Vitest for unit tests
- Use Playwright for E2E tests
- Test user flows: auth, sync, view activities

---

## Environment Variables

### Backend (appsettings.json / Environment)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://..."
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "pacelab-api",
    "Audience": "pacelab-web",
    "ExpirationMinutes": 60
  },
  "Strava": {
    "ClientId": "your-strava-client-id",
    "ClientSecret": "your-strava-client-secret",
    "RedirectUri": "https://api.pacelab.com/api/strava/callback"
  },
  "Weather": {
    "ApiKey": "your-weather-api-key",
    "BaseUrl": "https://api.openweathermap.org/data/2.5"
  }
}
```

### Frontend (.env.local)
```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

---

## Common Patterns & Best Practices

### Pagination Pattern
```csharp
public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

// Usage
var query = _context.Activities.Where(a => a.UserId == userId);
var total = await query.CountAsync();
var activities = await query
    .Skip((page - 1) * limit)
    .Take(limit)
    .ToListAsync();

return new PagedResponse<ActivityResponse>
{
    Data = activities.Select(MapToDto).ToList(),
    Page = page,
    Limit = limit,
    Total = total,
    TotalPages = (int)Math.Ceiling(total / (double)limit)
};
```

### Error Handling
```csharp
// Controller
try
{
    var result = await _service.GetActivity(userId, activityId);
    if (result == null)
        return NotFound(new { error = "Activity not found" });
    return Ok(result);
}
catch (UnauthorizedException ex)
{
    return Unauthorized(new { error = ex.Message });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error fetching activity");
    return StatusCode(500, new { error = "Internal server error" });
}
```

### JWT Token Extraction
```csharp
// Helper method
protected Guid GetUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim))
        throw new UnauthorizedException("User not authenticated");
    return Guid.Parse(userIdClaim);
}

// Usage
var userId = GetUserId();
```

### Unit Conversion
```csharp
// Helpers/UnitConverter.cs
public static class UnitConverter
{
    // Distance
    public static float MetersToMiles(float meters) => meters / 1609.34f;
    public static float MetersToKilometers(float meters) => meters / 1000f;
    
    // Speed
    public static float MpsToMph(float mps) => mps * 2.237f;
    public static float MpsToKph(float mps) => mps * 3.6f;
    public static float MpsToMinPerMile(float mps) => 26.8224f / mps;
    public static float MpsToMinPerKm(float mps) => 16.6667f / mps;
    
    // Temperature
    public static float CelsiusToFahrenheit(float celsius) => (celsius * 9 / 5) + 32;
    public static float FahrenheitToCelsius(float fahrenheit) => (fahrenheit - 32) * 5 / 9;
}
```

---

## Security Considerations

1. **Password Hashing:** Use BCrypt with work factor 12
2. **JWT Secret:** Store in environment variables, minimum 256 bits
3. **CORS:** Only allow specific origins (localhost:3000, pacelab.com)
4. **SQL Injection:** Use parameterized queries (EF Core handles this)
5. **API Keys:** Never commit to Git, use environment variables
6. **Rate Limiting:** Implement for auth endpoints (prevent brute force)
7. **Input Validation:** Validate all user input at controller level
8. **Authorization:** Always check `userId` matches authenticated user

---

## Performance Optimization

1. **Database Indexes:** Added on frequently queried columns (see schema)
2. **Eager Loading:** Use `.Include()` to avoid N+1 queries
3. **Pagination:** Always paginate list endpoints (default limit: 20)
4. **Caching:** Weather data never changes - fetch once and store
5. **Background Jobs:** Use Hangfire for long-running operations (sync, calculations)
6. **Connection Pooling:** Built-in with Npgsql and Neon

---

## Deployment Strategy

### Backend
1. Build Docker image or publish to folder
2. Deploy to Railway/Render/Azure
3. Set environment variables (connection string, JWT secret, API keys)
4. Run migrations: `dotnet ef database update`
5. Configure CORS for production domain

### Frontend
1. Build Next.js: `npm run build`
2. Deploy to Vercel (auto from Git)
3. Set `NEXT_PUBLIC_API_URL` to production API URL
4. Vercel handles SSL, CDN, and edge network automatically

---

## Future Features (Post-MVP)

- Training zones and heart rate analysis
- Route matching (find similar routes)
- Performance trends and charts
- Goal setting and tracking
- Social features (share activities, leaderboards)
- Mobile app (React Native)
- PDF export of training summaries
- Weather forecast integration (plan training)
- Garmin integration (in addition to Strava)

---

## Known Limitations & Trade-offs

1. **Weather Data Accuracy:** Historical weather is approximate, not exact
2. **Adjustment Algorithm:** Simplified model, not scientifically validated
3. **GPS Precision:** Consumer GPS is ±5-10 meters, affects calculations
4. **Strava Rate Limits:** 100 requests per 15 min, throttle sync if needed
5. **No Offline Support:** Requires internet connection
6. **Single Region Database:** May have latency for users far from AWS US East

---

## Common Issues & Solutions

**Issue:** Strava token expired
**Solution:** Check `expires_at`, refresh token before API call

**Issue:** Weather data not found for old activities
**Solution:** Some weather APIs only have limited historical data

**Issue:** EF Core migrations conflict
**Solution:** Drop database (dev only), delete migrations, recreate from scratch

**Issue:** CORS error in frontend
**Solution:** Check API CORS configuration includes frontend URL

**Issue:** JWT token invalid
**Solution:** Verify secret key matches between token generation and validation

---

## Development Workflow

1. **Start Backend:** `dotnet run` (runs on localhost:5000)
2. **Start Frontend:** `npm run dev` (runs on localhost:3000)
3. **Database Migrations:** `dotnet ef migrations add MigrationName` then `dotnet ef database update`
4. **Test Strava Integration:** Use Strava's sandbox mode for development
5. **Monitor Background Jobs:** Hangfire dashboard at `/hangfire`

---

## Key GitHub Copilot Prompts

When writing code, use these patterns:

```
"Create a controller endpoint for getting paginated activities"
"Write a service method that syncs activities from Strava"
"Generate a DTO for activity responses with unit conversion properties"
"Create an EF Core migration for the activities table with indexes"
"Write a Next.js page that displays a list of activities with TanStack Query"
"Create a Tailwind component for displaying activity cards with weather info"
"Generate a background job that runs every 15 minutes to sync all users"
"Write integration tests for the activities controller"
```

---

## Project Goals & Success Metrics

**Primary Goal:** Help athletes understand their true effort level regardless of environmental conditions

**Success Metrics:**
- Users connect Strava and see adjusted paces
- Users return weekly to check progress
- Users share insights ("My ride was actually 5% harder than it looked!")
- High engagement with dashboard and activity details
- Low bounce rate on landing page

**Non-Goals:**
- Social network features (MVP)
- Real-time activity tracking (use Strava for that)
- Training plan generation (maybe later)

---

## Contact & Resources

- **Strava API Docs:** https://developers.strava.com/docs/reference/
- **OpenWeather API:** https://openweathermap.org/api
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/
- **Next.js Docs:** https://nextjs.org/docs
- **shadcn/ui:** https://ui.shadcn.com/
- **TanStack Query:** https://tanstack.com/query/latest

---

## Additional notes
- Please update this instructions file with important information and architecture decisions as the project evolves.
- If you are unsure about a next step, ask questions before proceeding.
- Generate explainatory md files upon request and store them in the `docs/` directory.
