export interface MenuItem {
  module: string;
  label: string;
  icon: string;
  route: string;
  sortOrder: number;
  permissions: string[];
  children: MenuItem[];
}

export interface UserProfile {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  phone?: string;
  avatarPath?: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
}

export interface AuthResponse {
  userId: string;
  fullName: string;
  userName: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  tokenExpiresAt: string;
  permissions: string[];
  menuItems: MenuItem[];
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors: string[];
}
