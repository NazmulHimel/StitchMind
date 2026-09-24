const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5224";

export interface ApiRequestOptions extends Omit<RequestInit, "body"> {
  body?: any;
}

export async function apiRequest<T>(
  endpoint: string,
  options: ApiRequestOptions = {}
): Promise<{ success: boolean; data?: T; message?: string; errors?: string[] }> {
  const token = typeof window !== "undefined" ? localStorage.getItem("garments_access_token") : null;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const url = endpoint.startsWith("http") ? endpoint : `${API_BASE_URL}${endpoint}`;

  const fetchOptions: RequestInit = {
    ...options,
    headers,
    body: options.body !== undefined && typeof options.body !== "string"
      ? JSON.stringify(options.body)
      : options.body,
  };

  try {
    const res = await fetch(url, fetchOptions);
    const data = await res.json();
    return data;
  } catch (error: any) {
    return {
      success: false,
      errors: [error.message || "Network request failed"],
      message: "Network request failed",
    };
  }
}
