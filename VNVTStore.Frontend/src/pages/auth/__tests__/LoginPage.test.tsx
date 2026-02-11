import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import LoginPage from '../LoginPage';
import { authService } from '@/services/authService';
import { useAuthStore, useToast } from '@/store';

// Mock dependencies
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    externalLogin: vi.fn(),
  },
}));

vi.mock('@/store', () => ({
  useAuthStore: vi.fn(),
  useToast: vi.fn(),
}));

vi.mock('@/config/firebase', () => ({
  auth: {},
  googleProvider: {},
  facebookProvider: {},
}));

vi.mock('firebase/auth', () => ({
  signInWithPopup: vi.fn(),
  GoogleAuthProvider: vi.fn(),
  FacebookAuthProvider: vi.fn(),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

// Mock createSchemas
vi.mock('@/utils/schemas', () => ({
  createSchemas: () => ({
    loginSchema: {
        parse: () => {},
    }
  })
}));

// We need to bypass zodResolver for simple testing or mock it
// Actually, it's better to import zod and create real schema or mock useForm to avoid resolver issues
// But here we rely on the component using the resolver.
// A simple way is to mock zodResolver
vi.mock('@hookform/resolvers/zod', () => ({
  zodResolver: () => async (values: Record<string, unknown>) => {
    // Simple mock validation
    if (!values.email || !values.password) {
        return {
            values: {},
            errors: {
                email: !values.email ? { message: 'Required' } : undefined,
                password: !values.password ? { message: 'Required' } : undefined
            }
        };
    }
    return {
        values: values,
        errors: {}
    };
  }
}));


describe('LoginPage', () => {
  const mockLogin = vi.fn();
  const mockSuccess = vi.fn();
  const mockError = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthStore).mockReturnValue({
      login: mockLogin,
      isAuthenticated: false,
      user: null
    });

    vi.mocked(useToast).mockReturnValue({
      success: mockSuccess,
      error: mockError
    });
  });

  it('renders login form correctly', () => {
    render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    );

    expect(screen.getByTestId('email-input')).toBeInTheDocument();
    expect(screen.getByTestId('password-input')).toBeInTheDocument();
    expect(screen.getByTestId('login-button')).toBeInTheDocument();
  });

  it('handles successful login', async () => {
    vi.mocked(authService.login).mockResolvedValue({
      success: true,
      data: {
        token: 'token',
        refreshToken: 'refresh',
        user: { code: 'U1', email: 'test@example.com', role: 'Customer' }
      }
    });

    render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByTestId('email-input'), { target: { value: 'test@example.com' } });
    fireEvent.change(screen.getByTestId('password-input'), { target: { value: 'password' } });
    fireEvent.click(screen.getByTestId('login-button'));

    await waitFor(() => {
      expect(authService.login).toHaveBeenCalledWith({ username: 'test@example.com', password: 'password' });
      expect(mockLogin).toHaveBeenCalled();
      expect(mockSuccess).toHaveBeenCalled();
    });
  });

  it('handles failed login', async () => {
    vi.mocked(authService.login).mockResolvedValue({
      success: false,
      message: 'Invalid credentials'
    });

    render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByTestId('email-input'), { target: { value: 'test@example.com' } });
    fireEvent.change(screen.getByTestId('password-input'), { target: { value: 'wrong' } });
    fireEvent.click(screen.getByTestId('login-button'));

    await waitFor(() => {
        // Wait for error message to appear
        // Since error state is local state set on effect or async, we look for text
        expect(screen.getByText(/Invalid credentials/i)).toBeInTheDocument();
    });
  });
});
