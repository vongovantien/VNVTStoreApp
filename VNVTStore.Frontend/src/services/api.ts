import axios, { AxiosInstance, AxiosRequestConfig, AxiosError, InternalAxiosRequestConfig } from 'axios';

// Circular dependency fix: Inject store instead of importing it directly
let authStore: any = null;
export const injectStore = (store: any) => {
  authStore = store;
};

/**
 * API Configuration and Axios Instance
 * Base configuration for calling VNVTStore API
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';

// ============ Constants ============
export const HttpStatus = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const;

// ============ Types ============
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  statusCode: number;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
}

export enum SearchCondition {
  Equal = 0,
  NotEqual = 1,
  Contains = 2,
  GreaterThan = 3,
  GreaterThanEqual = 4,
  LessThan = 5,
  LessThanEqual = 6,
  DateTimeRange = 7,
  DayPart = 8,
  MonthPart = 9,
  DatePart = 10,
  IsNull = 11,
  IsNotNull = 12,
  In = 13,
  NotIn = 14,
  EqualExact = 15
}

export interface SearchDTO {
  field: string;
  operator: SearchCondition;
  value: any;
}

export interface SortDTO {
  sortBy: string;
  sortDescending: boolean;
}

export interface RequestDTO {
  pageIndex: number;
  pageSize: number;
  searching?: SearchDTO[];
  sortDTO?: SortDTO;
  fields?: string[];
}

// ============ Axios Setup ============

const axiosInstance: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // 30 seconds timeout
  headers: {
    'Content-Type': 'application/json',
    'Accept-Language': 'vi', // Default to Vietnamese
  },
  withCredentials: true // For XSRF protection if backend supports it
});

// Request Interceptor: Attach Token
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = authStore?.getState()?.token;
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`; // Use standard 'Bearer' prefix
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Variables for 401 Refresh Logic
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: Handle Errors & Refresh Token
axiosInstance.interceptors.response.use(
  (response) => {
    // Return data directly if needed, or keeping full response based on preference.
    // However, existing code expects specific wrapper/handling.
    // Axios puts body in `data`.
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as any;

    if (error.response) {
      switch (error.response.status) {
        case HttpStatus.UNAUTHORIZED: // 401
          // Special handling for Login/Refresh endpoints to avoid infinite loops
          if (originalRequest.url?.includes('auth/login') || originalRequest.url?.includes('auth/refresh-token')) {
            console.error("Phiên đăng nhập hết hạn hoặc không hợp lệ.");
            authStore?.getState()?.logout();
            return Promise.reject(error);
          }

          if (!originalRequest._retry) {
            if (isRefreshing) {
              return new Promise(function (resolve, reject) {
                failedQueue.push({ resolve, reject });
              })
                .then((token) => {
                  originalRequest.headers['Authorization'] = 'Bearer ' + token;
                  return axiosInstance(originalRequest);
                })
                .catch((err) => {
                  return Promise.reject(err);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
              const state = authStore?.getState();
              if (!state) throw new Error('Store not initialized');
              const { token, refreshToken, setTokens } = state;

              if (!token || !refreshToken) {
                // No tokens to refresh, just logout
                console.error("Token hết hạn, đang chuyển hướng đăng nhập...");
                throw new Error('No tokens available');
              }

              const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
                token,
                refreshToken
              });

              const { data } = response;
              if (data.success && data.data) {
                setTokens(data.data.token, data.data.refreshToken);
                axiosInstance.defaults.headers.common['Authorization'] = 'Bearer ' + data.data.token;
                processQueue(null, data.data.token);

                originalRequest.headers['Authorization'] = 'Bearer ' + data.data.token;
                return axiosInstance(originalRequest);
              } else {
                console.error("Làm mới token thất bại.");
                throw new Error('Refresh failed');
              }
            } catch (err) {
              processQueue(err, null);
              authStore?.getState()?.logout();
              return Promise.reject(err);
            } finally {
              isRefreshing = false;
            }
          }
          break;

        case HttpStatus.FORBIDDEN: // 403
          console.error("Bạn không có quyền truy cập tính năng này.");
          break;

        case HttpStatus.INTERNAL_SERVER_ERROR: // 500
          console.error("Lỗi server, vui lòng thử lại sau.");
          break;

        case HttpStatus.NOT_FOUND: // 404
          console.error("Không tìm thấy tài nguyên yêu cầu.");
          break;

        default:
          console.error(`Đã có lỗi xảy ra (${error.response.status}):`, error.response.data || error.message);
      }
    } else {
      console.error("Lỗi mạng hoặc server không phản hồi.");
    }

    return Promise.reject(error);
  }
);


// ============ API Client Adapter ============
// Maintains compatibility with existing code calling `apiClient.get<T>`
class ApiClient {

  async request<T>(
    method: string,
    endpoint: string,
    data?: unknown,
    options?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    try {
      const response = await axiosInstance.request({
        method,
        url: endpoint,
        data,
        ...options
      });

      const responseData = response.data;

      // Global Validation: Ensure response matches ApiResponse<T> (ResponseDTO) structure
      if (responseData && typeof responseData === 'object' && 'success' in responseData) {
        return responseData as ApiResponse<T>;
      }

      // If response is not a valid DTO (e.g. raw HTML or unexpected JSON), treat as error
      return {
        success: false,
        message: 'Invalid API Response Format', // Or localized message
        data: null,
        statusCode: response.status
      };
    } catch (error: unknown) {
      // Unify error format for frontend consumption
      let msg = 'Unknown error';
      let statusCode: number = HttpStatus.INTERNAL_SERVER_ERROR;

      if (axios.isAxiosError(error)) {
        msg = error.response?.data?.message || (error.message as string);
        statusCode = error.response?.status || HttpStatus.INTERNAL_SERVER_ERROR;
      } else if (error instanceof Error) {
        msg = error.message;
      }

      return {
        success: false,
        message: msg,
        data: null,
        statusCode: statusCode
      };
    }
  }

  get<T>(endpoint: string, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('GET', endpoint, undefined, options);
  }

  post<T>(endpoint: string, data?: unknown, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('POST', endpoint, data, options);
  }

  put<T>(endpoint: string, data?: unknown, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('PUT', endpoint, data, options);
  }

  delete<T>(endpoint: string, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('DELETE', endpoint, undefined, options);
  }

  // Expose axios instance if needed for advanced usage
  get instance() {
    return axiosInstance;
  }
}

export const apiClient = new ApiClient();
export default apiClient;
