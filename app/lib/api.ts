import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  PagedResponse,
  ActivityResponse,
  UpdateActivityRequest,
  SyncStatusResponse,
  SyncHistoryResponse,
  SyncTestResult,
  UserStravaStatus,
  StravaSyncResult,
  StravaAuthorizeResponse,
} from "./types";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api";

// ─── Token helpers ────────────────────────────────────────────────────────────

const TOKEN_KEY = "pacelab_token";

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function removeToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

// ─── Core fetch helper ────────────────────────────────────────────────────────

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}`;
    try {
      const errorBody = await response.json();
      if (errorBody?.message) message = errorBody.message;
      else if (errorBody?.error) message = errorBody.error;
    } catch {
      // ignore JSON parse errors on error bodies
    }
    throw new Error(message);
  }

  // 204 No Content — return undefined cast to T
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

// ─── Auth ─────────────────────────────────────────────────────────────────────

export const auth = {
  /** Register a new user and store the returned JWT. */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const res = await request<AuthResponse>("/auth/register", {
      method: "POST",
      body: JSON.stringify(data),
    });
    setToken(res.token);
    return res;
  },

  /** Log in with email/password and store the returned JWT. */
  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await request<AuthResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify(data),
    });
    setToken(res.token);
    return res;
  },

  /** Fetch the currently authenticated user's profile. */
  async me(): Promise<AuthResponse["user"]> {
    return request("/auth/me");
  },

  /** Remove the stored JWT (client-side logout). */
  logout(): void {
    removeToken();
  },
};

// ─── Activities ───────────────────────────────────────────────────────────────

export const activities = {
  /** Get a paginated list of activities, optionally filtered by type. */
  async list(
    page = 1,
    limit = 20,
    type?: string
  ): Promise<PagedResponse<ActivityResponse>> {
    const params = new URLSearchParams({
      page: String(page),
      limit: String(limit),
    });
    if (type) params.set("type", type);
    return request(`/activities?${params.toString()}`);
  },

  /** Get a single activity by ID. */
  async get(id: number): Promise<ActivityResponse> {
    return request(`/activities/${id}`);
  },

  /** Update an activity's name and/or type. */
  async update(id: number, data: UpdateActivityRequest): Promise<ActivityResponse> {
    return request(`/activities/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  /** Delete a single activity by ID. */
  async remove(id: number): Promise<void> {
    return request(`/activities/${id}`, { method: "DELETE" });
  },

  /** Delete all activities for the current user. */
  async removeAll(): Promise<void> {
    return request("/activities", { method: "DELETE" });
  },

  /** Recalculate effort adjustments for an activity. */
  async recalculate(activityId: number): Promise<ActivityResponse> {
    return request(`/activities/${activityId}/recalculate`, {
      method: "POST",
    });
  },
};

// ─── Sync ─────────────────────────────────────────────────────────────────────

export const sync = {
  /** Get the current Strava sync status and activity totals. */
  async status(): Promise<SyncStatusResponse> {
    return request("/sync/status");
  },

  /** Get paginated sync history. */
  async history(
    page = 1,
    limit = 20
  ): Promise<PagedResponse<SyncHistoryResponse>> {
    const params = new URLSearchParams({
      page: String(page),
      limit: String(limit),
    });
    return request(`/sync/history?${params.toString()}`);
  },

  /** Test the Strava connection without persisting any data. */
  async test(): Promise<SyncTestResult> {
    return request("/sync/test", { method: "POST" });
  },
};

// ─── Strava ───────────────────────────────────────────────────────────────────

export const strava = {
  /** Get the Strava OAuth authorization URL to redirect the user to. */
  async getAuthorizeUrl(): Promise<StravaAuthorizeResponse> {
    return request("/strava/authorize");
  },

  /** Check the current Strava connection status. */
  async status(): Promise<UserStravaStatus> {
    return request("/strava/status");
  },

  /** Trigger a Strava activity sync for the current user. */
  async sync(): Promise<StravaSyncResult> {
    return request("/strava/sync", { method: "POST" });
  },

  /** Disconnect the current user's Strava account. */
  async disconnect(): Promise<void> {
    return request("/strava/disconnect", { method: "DELETE" });
  },
};

// ─── Default export ───────────────────────────────────────────────────────────

const api = { auth, activities, sync, strava };
export default api;
