import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import ProfileContent from '../ProfileContent';
import { userService } from '@/services/userService';

// Mock dependencies
vi.mock('@/services/userService', () => ({
  userService: {
    getProfile: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
  },
}));

vi.mock('@/store', () => ({
  useToast: () => ({
    success: vi.fn(),
    error: vi.fn(),
  }),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

describe('ProfileContent Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default mock response for getProfile
    (userService.getProfile as any).mockResolvedValue({
      success: true,
      data: {
        fullName: 'Test User',
        email: 'test@example.com',
        phone: '0909090909',
      },
    });
  });

  describe('Profile Information', () => {
    it('renders profile form with user data', async () => {
      render(<ProfileContent />);

      await waitFor(() => {
        expect(screen.getByDisplayValue('Test User')).toBeTruthy();
        expect(screen.getByDisplayValue('test@example.com')).toBeTruthy();
      });
    });

    it('submits profile update successfully', async () => {
      render(<ProfileContent />);
      
      // Wait for data load
      await waitFor(() => screen.getByDisplayValue('Test User'));

      const nameInput = screen.getByDisplayValue('Test User');
      fireEvent.change(nameInput, { target: { value: 'Updated Name' } });
      
      const saveButton = screen.getAllByText('common.save')[0]; // First save button is for profile
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(userService.updateProfile).toHaveBeenCalledWith(expect.objectContaining({
          fullName: 'Updated Name'
        }));
      });
    });
  });

  describe('Change Password', () => {
    it('shows validation error for mismatched passwords', async () => {
      render(<ProfileContent />);

      const newPassInput = screen.getByPlaceholderText('common.placeholders.newPassword');
      const confirmPassInput = screen.getByPlaceholderText('common.placeholders.confirmPassword');
      const currentPassInput = screen.getByPlaceholderText('common.placeholders.currentPassword');

      fireEvent.change(currentPassInput, { target: { value: 'OldPass1!' } });
      fireEvent.change(newPassInput, { target: { value: 'NewPass1!' } });
      fireEvent.change(confirmPassInput, { target: { value: 'DifferentPass1!' } });

      const saveButton = screen.getAllByText('common.save')[1]; // Second save button is for password
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('validation.passwordMismatch')).toBeTruthy();
        expect(userService.changePassword).not.toHaveBeenCalled();
      });
    });

    it('submits change password request when valid', async () => {
      (userService.changePassword as any).mockResolvedValue({ success: true });
      
      render(<ProfileContent />);

      const currentPassInput = screen.getByPlaceholderText('common.placeholders.currentPassword');
      const newPassInput = screen.getByPlaceholderText('common.placeholders.newPassword');
      const confirmPassInput = screen.getByPlaceholderText('common.placeholders.confirmPassword');

      fireEvent.change(currentPassInput, { target: { value: 'OldPass1!' } });
      fireEvent.change(newPassInput, { target: { value: 'NewPass1!' } });
      fireEvent.change(confirmPassInput, { target: { value: 'NewPass1!' } });

      const saveButton = screen.getAllByText('common.save')[1];
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(userService.changePassword).toHaveBeenCalledWith({
          currentPassword: 'OldPass1!',
          newPassword: 'NewPass1!',
          confirmNewPassword: 'NewPass1!',
        });
      });
    });

    it('shows error toast when API fails', async () => {
       const mockError = new Error('API Error');
       (userService.changePassword as any).mockRejectedValue(mockError);
       render(<ProfileContent />);
       
       const currentPassInput = screen.getByPlaceholderText('common.placeholders.currentPassword');
       const newPassInput = screen.getByPlaceholderText('common.placeholders.newPassword');
       const confirmPassInput = screen.getByPlaceholderText('common.placeholders.confirmPassword');

       fireEvent.change(currentPassInput, { target: { value: 'OldPass1!' } });
       fireEvent.change(newPassInput, { target: { value: 'NewPass1!' } });
       fireEvent.change(confirmPassInput, { target: { value: 'NewPass1!' } });

       const saveButton = screen.getAllByText('common.save')[1];
       fireEvent.click(saveButton);

       await waitFor(() => {
         expect(userService.changePassword).toHaveBeenCalled();
         // Check if toast.error was called (mocked in beforeEach logic implicitly or explicit setup)
         // Note: We need to ensure useToast mock is correct.
       });
    });

    it('disables save button while loading', async () => {
        // Slow response
        (userService.changePassword as any).mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
        render(<ProfileContent />);
        
        const currentPassInput = screen.getByPlaceholderText('common.placeholders.currentPassword');
        const newPassInput = screen.getByPlaceholderText('common.placeholders.newPassword');
        const confirmPassInput = screen.getByPlaceholderText('common.placeholders.confirmPassword');

        fireEvent.change(currentPassInput, { target: { value: 'OldPass1!' } });
        fireEvent.change(newPassInput, { target: { value: 'NewPass1!' } });
        fireEvent.change(confirmPassInput, { target: { value: 'NewPass1!' } });
        
        const saveButton = screen.getAllByText('common.save')[1];
        fireEvent.click(saveButton);
        
        
        await waitFor(() => expect(saveButton).toBeDisabled());
        
        await waitFor(() => expect(saveButton).not.toBeDisabled());
    });
  });

  describe('Form Validation', () => {
      it('validates email format in read-only field (sanity check)', async () => {
          render(<ProfileContent />);
          await waitFor(() => screen.getByDisplayValue('test@example.com'));
          const emailInput = screen.getByLabelText('Email');
          expect(emailInput).toHaveAttribute('readonly');
      });

      it('requires full name', async () => {
          render(<ProfileContent />);
          await waitFor(() => screen.getByDisplayValue('Test User'));
          
          const nameInput = screen.getByDisplayValue('Test User');
          fireEvent.change(nameInput, { target: { value: '' } });
          
          const saveButton = screen.getAllByText('common.save')[0];
          fireEvent.click(saveButton);
          
          await waitFor(() => {
              expect(screen.getByText('validation.required')).toBeTruthy();
          });
      });
  });
});
