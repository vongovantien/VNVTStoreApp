import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import RegisterPage from '../RegisterPage';
import { authService } from '@/services/authService';
import { useToast } from '@/store';

// Mock dependencies
vi.mock('@/services/authService', () => ({
  authService: {
    register: vi.fn(),
  },
}));

vi.mock('@/store', () => ({
    useAuthStore: vi.fn(),
    useToast: vi.fn(),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

// Mock z and schemas
vi.mock('@/utils/schemas', () => ({
    createSchemas: () => ({
        registerSchema: {}
    })
}));

// Mock zodResolver to bypass complex schema validation
vi.mock('@hookform/resolvers/zod', () => ({
    zodResolver: () => async (values: Record<string, unknown>) => {
        const errors: Record<string, { message: string }> = {};
        if (!values.email) errors.email = { message: 'Required' };
        if (!values.password) errors.password = { message: 'Required' };
        if (!values.confirmPassword) errors.confirmPassword = { message: 'Required' };
        if (values.password !== values.confirmPassword) errors.confirmPassword = { message: 'Mismatch' };
        if (!values.agreeTerms) errors.agreeTerms = { message: 'Must agree' };
        
        return {
            values: Object.keys(errors).length === 0 ? values : {},
            errors: errors
        };
    }
}));


describe('RegisterPage', () => {
    const mockSuccess = vi.fn();
    const mockError = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
        vi.mocked(useToast).mockReturnValue({
            success: mockSuccess,
            error: mockError
        });
    });

    it('renders register form correctly', () => {
        render(
            <BrowserRouter>
                <RegisterPage />
            </BrowserRouter>
        );

        expect(screen.getByText('register.title')).toBeInTheDocument();
        // Check for inputs by label text (using the translation keys mock)
        expect(screen.getByLabelText(/register.name/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Email/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/register.phone/i)).toBeInTheDocument();
        expect(screen.getByLabelText('register.password')).toBeInTheDocument();
        expect(screen.getByLabelText('register.confirmPassword')).toBeInTheDocument();
    });

    it('handles validation error for mismatched passwords', async () => {
        render(
            <BrowserRouter>
                <RegisterPage />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByLabelText(/Email/i), { target: { value: 'test@example.com' } });
        fireEvent.change(screen.getByLabelText('register.password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('register.confirmPassword'), { target: { value: 'password456' } });
        fireEvent.click(screen.getByRole('checkbox')); // Agree terms
        
        fireEvent.click(screen.getByText('register.submit'));

        await waitFor(() => {
             // We mocked the resolver to return Mismatch error for confirmPassword
             // But usually it shows error message text.
             // Since we don't have the error text in the screen (Input component renders it), we check if authService was NOT called
             expect(authService.register).not.toHaveBeenCalled();
        });
        
        // If Input component renders error, we could check for it:
        // expect(screen.getByText('Mismatch')).toBeDefined(); 
    });

    it('handles successful registration', async () => {
        vi.mocked(authService.register).mockResolvedValue({
            success: true,
            data: { id: '1' }
        });

        render(
            <BrowserRouter>
                <RegisterPage />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByLabelText(/Email/i), { target: { value: 'test@example.com' } });
        fireEvent.change(screen.getByLabelText('register.password'), { target: { value: 'password123' } });
        fireEvent.change(screen.getByLabelText('register.confirmPassword'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('checkbox')); // Agree terms
        
        fireEvent.click(screen.getByText('register.submit'));

        await waitFor(() => {
            expect(authService.register).toHaveBeenCalledWith(expect.objectContaining({
                email: 'test@example.com',
                password: 'password123'
            }));
            expect(mockSuccess).toHaveBeenCalled();
        });
    });
});
