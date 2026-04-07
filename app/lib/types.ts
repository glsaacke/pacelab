// ─── Auth ────────────────────────────────────────────────────────────────────

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserResponse {
  userId: number;
  email: string;
  createdAt: string | null;
  lastLoggedIn: string | null;
}

export interface AuthResponse {
  user: UserResponse;
  token: string;
}

// ─── Pagination ───────────────────────────────────────────────────────────────

export interface PagedResponse<T> {
  data: T[];
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

// ─── Activities ───────────────────────────────────────────────────────────────

export interface ActivityResponse {
  activityId: number;
  userId: number;
  stravaActivityId: number | null;
  activityType: string | null;
  activityName: string | null;
  startDate: string | null;
  timezone: string | null;
  distanceMeters: number | null;
  movingTimeSeconds: number | null;
  elapsedTimeSeconds: number | null;
  totalElevationGain: number | null;
  avgSpeed: number | null;
  maxSpeed: number | null;
  avgHeartrate: number | null;
  maxHeartrate: number | null;
  avgWatts: number | null;
  calories: number | null;
  startLatitude: number | null;
  startLongitude: number | null;
  endLatitude: number | null;
  endLongitude: number | null;
  polyline: string | null;
  altitude: number | null;
  createdAt: string | null;
  updatedAt: string | null;
  // Computed unit-conversion properties returned by the backend
  distanceMiles: number | null;
  distanceKilometers: number | null;
  avgSpeedMph: number | null;
  avgSpeedKph: number | null;
  avgPaceMinPerMile: number | null;
  avgPaceMinPerKm: number | null;
  // Related data (included when available)
  weather: ActivityWeatherResponse | null;
  adjustments: ActivityAdjustmentsResponse | null;
}

export interface UpdateActivityRequest {
  activityName?: string | null;
  activityType?: string | null;
}

// ─── Weather ──────────────────────────────────────────────────────────────────

export interface ActivityWeatherResponse {
  activityWeatherId: number;
  activityId: number;
  temperatureCelsius: number | null;
  feelsLikeCelsius: number | null;
  windSpeedMps: number | null;
  windDirectionDegrees: number | null;
  humidityPercent: number | null;
  precipitationMm: number | null;
  weatherCondition: string | null;
  fetchedAt: string | null;
}

// ─── Adjustments ─────────────────────────────────────────────────────────────

export interface ActivityAdjustmentsResponse {
  activityAdjustmentsId: number;
  activityId: number;
  adjustedSpeedMps: number | null;
  adjustedTimeSeconds: number | null;
  windAdjustment: number | null;
  heatAdjustment: number | null;
  coldAdjustment: number | null;
  precipitationAdjustment: number | null;
  elevationAdjustment: number | null;
  elevationGainAdjustment: number | null;
  totalAdjustment: number | null;
  totalAdjustmentPercent: number | null;
  difficultyRating: string | null;
  calculatedAt: string | null;
}

// ─── Sync ─────────────────────────────────────────────────────────────────────

export interface SyncStatusResponse {
  stravaConnected: boolean;
  lastSyncAt: string | null;
  lastSyncStatus: string | null;
  lastSyncActivitiesSynced: number | null;
  lastSyncError: string | null;
  totalActivities: number;
}

export interface SyncHistoryResponse {
  syncLogId: number;
  syncType: string | null;
  status: string | null;
  activitiesSynced: number | null;
  errorMessage: string | null;
  startedAt: string | null;
  completedAt: string | null;
  durationSeconds: number | null;
}

export interface SyncTestResult {
  success: boolean;
  stravaConnected: boolean;
  tokenValid: boolean;
  stravaUsername: string | null;
  errorMessage: string | null;
  testedAt: string;
}

// ─── Strava ───────────────────────────────────────────────────────────────────

export interface UserStravaStatus {
  connected: boolean;
  stravaUserId: number | null;
  stravaUsername: string | null;
  tokenExpiresAt: string | null;
}

export interface StravaSyncResult {
  activitiesFound: number;
  activitiesSkipped: number;
  activitiesSynced: number;
  weatherFetched: number;
  adjustmentsCalculated: number;
  syncedAt: string;
  errorMessage: string | null;
}

export interface StravaAuthorizeResponse {
  authorizeUrl: string;
}
