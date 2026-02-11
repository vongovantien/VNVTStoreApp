import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ImportModal } from '../ImportModal';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock dependencies if needed (e.g., Lucide icons usually render fine, but sometimes need mocking if they use specific browser APIs not in JSDOM)

describe('ImportModal', () => {
    const mockOnClose = vi.fn();
    const mockOnImport = vi.fn();

    const defaultProps = {
        isOpen: true,
        onClose: mockOnClose,
        onImport: mockOnImport,
        title: 'Test Import',
        templateUrl: '/test-template',
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('should verify that the modal is not rendered when isOpen is false', () => {
        render(<ImportModal {...defaultProps} isOpen={false} />);
        expect(screen.queryByText('Test Import')).not.toBeInTheDocument();
    });

    it('should verify that the modal is rendered when isOpen is true', () => {
        render(<ImportModal {...defaultProps} />);
        expect(screen.getByText('Test Import')).toBeInTheDocument();
        expect(screen.getByText('Import Data')).toBeInTheDocument();
    });

    it('should verify file selection via file input', async () => {
        render(<ImportModal {...defaultProps} />);
        
        const file = new File(['dummy content'], 'test.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const input = screen.getByTestId('dropzone-input');

        await userEvent.upload(input, file);

        expect(screen.getByText('test.xlsx')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /remove file/i })).toBeInTheDocument();
    });

    it('should verify calling onImport when a file is selected and import button is clicked', async () => {
        mockOnImport.mockResolvedValue(undefined); // Mock successful import
        render(<ImportModal {...defaultProps} />);

        const file = new File(['dummy content'], 'test.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const input = screen.getByTestId('dropzone-input');

        // Select file
        await userEvent.upload(input, file);

        // Click Import
        const importBtn = screen.getByRole('button', { name: /import data/i });
        expect(importBtn).toBeEnabled();
        
        await userEvent.click(importBtn);

        expect(mockOnImport).toHaveBeenCalledTimes(1);
        expect(mockOnImport).toHaveBeenCalledWith(file);
        
        // Should close after success
        await waitFor(() => {
            expect(mockOnClose).toHaveBeenCalledTimes(1);
        });
    });

    it('should verify that loading state is shown during import', async () => {
        // Mock import that takes some time
        mockOnImport.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
        render(<ImportModal {...defaultProps} />);

        const file = new File(['dummy content'], 'test.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const input = screen.getByTestId('dropzone-input');
        await userEvent.upload(input, file);

        const importBtn = screen.getByRole('button', { name: /import data/i });
        await userEvent.click(importBtn);

        // Verify loading state
        expect(importBtn).toBeDisabled(); // Button usually disabled or shows spinner
        // Assuming your Button component has an isLoading prop that disables it
    });
});
