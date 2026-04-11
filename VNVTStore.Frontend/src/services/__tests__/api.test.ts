import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import axios from 'axios';
import { apiClient, injectStore, HttpStatus } from '../api';

// Smart mock of axios that captures interceptor handlers and is CALLABLE
vi.mock('axios', async (importOriginal) => {
  const actual: any = await importOriginal();
  
  // Make the mock itself a callable function (like axios() or instance())
  const mockAxios: any = vi.fn((config) => {
    return mockAxios.request(config);
  });

  // Attach methods to the callable mock
  mockAxios.request = vi.fn();
  mockAxios.get = vi.fn();
  mockAxios.post = vi.fn();
  mockAxios.put = vi.fn();
  mockAxios.delete = vi.fn();
  mockAxios.defaults = { headers: { common: {} } };
  mockAxios.isAxiosError = actual.isAxiosError;
  
  mockAxios.interceptors = {
    request: {
      use: vi.fn((fulfilled, rejected) => {
        mockAxios.interceptors.request.handlers.push({ fulfilled, rejected });
      }),
      eject: vi.fn(),
      handlers: []
    },
    response: {
      use: vi.fn((fulfilled, rejected) => {
        mockAxios.interceptors.response.handlers.push({ fulfilled, rejected });
      }),
      eject: vi.fn(),
      handlers: []
    }
  };

  mockAxios.create = vi.fn(() => mockAxios);

  return {
    default: mockAxios,
    ...mockAxios,
  };
});

describe('ApiClient & Interceptors', () => {
  let mockStore: any;
  let mockState: any;

  beforeEach(() => {
    vi.clearAllMocks();
    
    // Create a stable state object
    mockState = {
      token: 'old-token',
      refreshToken: 'refresh-token',
      setTokens: vi.fn(),
      logout: vi.fn(),
    };

    mockStore = {
      getState: vi.fn(() => mockState),
    };
    injectStore(mockStore);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should attach Authorization header from store', async () => {
    const handlers = (apiClient.instance.interceptors.request as any).handlers;
    const requestInterceptor = handlers[0].fulfilled;

    const config = { headers: {} } as any;
    const result = requestInterceptor(config);

    expect(result.headers.Authorization).toBe('Bearer old-token');
  });

  it('should handle 401 by calling refresh-token if not retried', async () => {
    const handlers = (apiClient.instance.interceptors.response as any).handlers;
    const responseInterceptorError = handlers[0].rejected;

    const originalRequest = { url: '/test', headers: {}, _retry: false };
    const error = {
      config: originalRequest,
      response: { status: HttpStatus.UNAUTHORIZED },
      isAxiosError: true,
    } as any;

    // Mock successful refresh token call
    (axios.post as any).mockResolvedValue({
      data: { success: true, data: { token: 'new-token', refreshToken: 'new-refresh' } }
    });

    // Mock the instance call for retry (the callable mock)
    (apiClient.instance as any).mockResolvedValue({ data: { success: true } });

    await responseInterceptorError(error);

    expect(axios.post).toHaveBeenCalledWith(expect.stringContaining('refresh-token'), expect.anything());
    expect(mockState.setTokens).toHaveBeenCalledWith('new-token', 'new-refresh');
  });

  it('should logout if refresh-token fails', async () => {
    const handlers = (apiClient.instance.interceptors.response as any).handlers;
    const responseInterceptorError = handlers[0].rejected;

    const originalRequest = { url: '/test', headers: {}, _retry: false };
    const error = {
      config: originalRequest,
      response: { status: HttpStatus.UNAUTHORIZED },
      isAxiosError: true,
    } as any;

    // Mock refresh token failure
    (axios.post as any).mockRejectedValue(new Error('Network error'));

    try {
      await responseInterceptorError(error);
    } catch (e) {
      // Expected
    }

    expect(mockState.logout).toHaveBeenCalled();
  });
});
