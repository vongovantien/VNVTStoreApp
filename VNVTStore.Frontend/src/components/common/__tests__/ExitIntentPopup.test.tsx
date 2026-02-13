/**
 * Unit tests for ExitIntentPopup component
 * Feature: #51 Exit Intent Popup
 */
import { render, screen, fireEvent, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, onClick, ...props }: any) => <div onClick={onClick} {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Gift: () => <span>Gift</span>,
  X: () => <span>X</span>,
  Copy: () => <span>Copy</span>,
  Check: () => <span>Check</span>,
}));

import { ExitIntentPopup } from '../ExitIntentPopup';

describe('ExitIntentPopup - Feature #51', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('does not show popup initially', () => {
    render(<ExitIntentPopup />);
    expect(screen.queryByText(/Khoan đã/)).not.toBeInTheDocument();
  });

  it('shows popup after delay + mouseout from top', async () => {
    render(<ExitIntentPopup />);
    
    // Advance past the 5-second delay
    await act(async () => {
      vi.advanceTimersByTime(6000);
    });

    // Simulate mouseout from top of viewport
    await act(async () => {
      fireEvent.mouseOut(document, { clientY: -1 });
    });

    expect(screen.getByText(/Khoan đã/)).toBeInTheDocument();
  });

  it('shows a coupon code in the popup', async () => {
    render(<ExitIntentPopup />);
    
    await act(async () => {
      vi.advanceTimersByTime(6000);
    });
    await act(async () => {
      fireEvent.mouseOut(document, { clientY: -1 });
    });

    expect(screen.getByText(/WELCOME10/)).toBeInTheDocument();
  });

  it('marks as shown in sessionStorage after displaying', async () => {
    render(<ExitIntentPopup />);
    
    await act(async () => {
      vi.advanceTimersByTime(6000);
    });
    await act(async () => {
      fireEvent.mouseOut(document, { clientY: -1 });
    });

    expect(sessionStorage.getItem('vnvt_exit_popup_shown')).toBe('true');
  });

  it('does not show popup if already shown in session', async () => {
    sessionStorage.setItem('vnvt_exit_popup_shown', 'true');
    render(<ExitIntentPopup />);
    
    await act(async () => {
      vi.advanceTimersByTime(6000);
    });
    await act(async () => {
      fireEvent.mouseOut(document, { clientY: -1 });
    });

    expect(screen.queryByText(/Khoan đã/)).not.toBeInTheDocument();
  });
});
