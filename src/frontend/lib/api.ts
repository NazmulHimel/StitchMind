const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5224";

export interface ApiRequestOptions extends Omit<RequestInit, "body"> {
  body?: any;
}

export interface ApiResponse<T = any> {
  isSuccess?: boolean;
  value?: T;
  success?: boolean;
  data?: T;
  message?: string;
  error?: string;
  errors?: string[];
}

export async function apiRequest<T = any>(
  endpoint: string,
  options: ApiRequestOptions = {}
): Promise<ApiResponse<T>> {
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
    if (data.isSuccess !== undefined && data.success === undefined) {
      data.success = data.isSuccess;
      data.data = data.value;
    }
    return data;
  } catch (error: any) {
    return {
      isSuccess: false,
      success: false,
      errors: [error.message || "Network request failed"],
      error: error.message || "Network request failed",
      message: "Network request failed",
    };
  }
}
