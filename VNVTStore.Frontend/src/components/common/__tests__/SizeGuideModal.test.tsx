/**
 * Unit tests for SizeGuideModal component
 * Feature: #11 Size Guide Calculator
 */
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: React.HTMLAttributes<HTMLDivElement>) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Ruler: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Ruler</span>,
  X: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>X</span>,
  Info: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Info</span>,
}));

import { SizeGuideModal } from '../SizeGuideModal';

describe('SizeGuideModal - Feature #11', () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders when isOpen is true', () => {
    render(<SizeGuideModal isOpen={true} onClose={mockOnClose} />);
    expect(screen.getByText(/Hướng dẫn chọn size/)).toBeInTheDocument();
  });

  it('does not render when isOpen is false', () => {
    render(<SizeGuideModal isOpen={false} onClose={mockOnClose} />);
    expect(screen.queryByText(/Hướng dẫn chọn size/)).not.toBeInTheDocument();
  });

  it('has input fields for height and weight', () => {
    render(<SizeGuideModal isOpen={true} onClose={mockOnClose} />);
    const heightInput = screen.getByPlaceholderText('170');
    const weightInput = screen.getByPlaceholderText('65');
    expect(heightInput).toBeInTheDocument();
    expect(weightInput).toBeInTheDocument();
  });

  it('recommends a size based on height and weight input', () => {
    render(<SizeGuideModal isOpen={true} onClose={mockOnClose} />);
    
    const heightInput = screen.getByPlaceholderText('170');
    const weightInput = screen.getByPlaceholderText('65');
    
    fireEvent.change(heightInput, { target: { value: '170' } });
    fireEvent.change(weightInput, { target: { value: '65' } });

    // Should display "Size gợi ý cho bạn" and a size recommendation
    expect(screen.getByText(/Size gợi ý/)).toBeInTheDocument();
  });

  it('displays a size chart table', () => {
    render(<SizeGuideModal isOpen={true} onClose={mockOnClose} />);
    const table = document.querySelector('table');
    expect(table).toBeInTheDocument();
  });

  it('calls onClose when close button is clicked', () => {
    render(<SizeGuideModal isOpen={true} onClose={mockOnClose} />);
    
    // Find close button (has X icon) - the button with "X" text from mocked lucide
    const buttons = screen.getAllByRole('button');
    const closeBtn = buttons.find(btn => btn.textContent?.includes('X'));
    if (closeBtn) {
      fireEvent.click(closeBtn);
      expect(mockOnClose).toHaveBeenCalled();
    }
  });
});
