import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ImportModal } from '../ImportModal';
import userEvent from '@testing-library/user-event';

// Mock dependencies
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, defaultValue: string) => defaultValue || key,
  }),
}));

describe('ImportModal', () => {
  const mockOnClose = vi.fn();
  const mockOnImport = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders correctly when open', () => {
    render(
      <ImportModal
        isOpen={true}
        onClose={mockOnClose}
        onImport={mockOnImport}
        title="Test Import"
      />
    );

    expect(screen.getByText('Test Import')).toBeInTheDocument();
    expect(screen.getByText('Drag and drop file here')).toBeInTheDocument();
  });

  it('calls onImport when a file is selected and import button is clicked', async () => {
    const user = userEvent.setup();
    render(
      <ImportModal
        isOpen={true}
        onClose={mockOnClose}
        onImport={mockOnImport}
        title="Test Import"
      />
    );

    const file = new File(['dummy content'], 'test.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const input = screen.getByTestId('dropzone-input') || screen.getByRole('presentation').querySelector('input');
    
    // Simulate file drop/select
    // Note: react-dropzone input is hidden. We need to target it.
    // The ImportModal implementation puts {...getInputProps()} on an input.
    // We can use fireEvent.change on the input.
    
    const hiddenInput = document.querySelector('input[type="file"]');
    if (hiddenInput) {
        await user.upload(hiddenInput as HTMLElement, file);
    }

    expect(screen.getByText('test.xlsx')).toBeInTheDocument();
    
    // Click Import
    const importBtn = screen.getByRole('button', { name: /Import Data/i });
    expect(importBtn).toBeEnabled();
    
    await user.click(importBtn);

    expect(mockOnImport).toHaveBeenCalledWith(file);
    expect(mockOnClose).toHaveBeenCalled();
  }, 10000); // Increase timeout to 10s

  it('disables import button when no file is selected', () => {
    render(
      <ImportModal
        isOpen={true}
        onClose={mockOnClose}
        onImport={mockOnImport}
        title="Test Import"
      />
    );

    const importBtn = screen.getByRole('button', { name: /Import Data/i });
    expect(importBtn).toBeDisabled();
  });
});
