export interface ApiResponse<T = any> {
  success: boolean;
  code: number;
  message: string;
  operation: string;
  data: T;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: number;
  email: string;
  role: string;
}

export interface ApiErrorResponse {
  success: false;
  code: number;
  message: string;
  operation: string;
  errors?: Record<string, string[]>;
}
