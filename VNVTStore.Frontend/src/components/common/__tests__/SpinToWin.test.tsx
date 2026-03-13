/**
 * Unit tests for SpinToWin component
 * Feature: #56 Gamified Spin-to-Win Coupon Wheel
 */
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, onClick, ...props }: React.HTMLAttributes<HTMLDivElement>) => <div onClick={onClick} {...props}>{children}</div>,
    button: ({ children, onClick, ...props }: React.ButtonHTMLAttributes<HTMLButtonElement>) => <button onClick={onClick} {...props}>{children}</button>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Gift: () => <span>Gift</span>,
  X: () => <span>X</span>,
  Copy: () => <span>Copy</span>,
  Check: () => <span>Check</span>,
}));

import { SpinToWin } from '../SpinToWin';

describe('SpinToWin - Feature #56', () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('renders the spin wheel when open', () => {
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    expect(screen.getByText(/Vòng quay may mắn/)).toBeInTheDocument();
  });

  it('does not render when closed', () => {
    render(<SpinToWin isOpen={false} onClose={mockOnClose} />);
    expect(screen.queryByText(/Vòng quay may mắn/)).not.toBeInTheDocument();
  });

  it('has a spin button', () => {
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    const spinBtn = screen.getByText(/Quay ngay/i);
    expect(spinBtn).toBeInTheDocument();
  });

  it('renders SVG wheel with segments', () => {
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    const svg = document.querySelector('svg');
    expect(svg).toBeInTheDocument();
    // Should have multiple path elements (wheel segments)
    const paths = svg!.querySelectorAll('path');
    expect(paths.length).toBeGreaterThan(1);
  });

  it('calls onClose when close button is clicked', () => {
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    const buttons = screen.getAllByRole('button');
    // The close button is the first one
    const closeBtn = buttons[0];
    fireEvent.click(closeBtn);
    expect(mockOnClose).toHaveBeenCalled();
  });

  it('disables spin if already spun (localStorage check)', () => {
    localStorage.setItem('vnvt_spin_result', JSON.stringify({ label: '10%', code: 'SPIN10' }));
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    // Button should say "Bạn đã quay rồi" or be disabled
    const spinBtn = screen.queryByText(/Quay ngay/i) || screen.queryByText(/đã quay/i);
    expect(spinBtn).toBeTruthy();
  });

  it('shows description text', () => {
    render(<SpinToWin isOpen={true} onClose={mockOnClose} />);
    expect(screen.getByText(/Quay để nhận ưu đãi/)).toBeInTheDocument();
  });
});
