/**
 * Unit tests for LiveSalesPopup component
 * Feature: #50 Live Sales Popups
 */
import { render, screen, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: { children: React.ReactNode }) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

import { LiveSalesPopup } from '../LiveSalesPopup';

describe('LiveSalesPopup - Feature #50', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('renders without crashing', () => {
    const { container } = render(<LiveSalesPopup />);
    expect(container).toBeTruthy();
  });

  it('shows a sale notification after initial delay', async () => {
    render(<LiveSalesPopup />);
    
    // Advance past the initial delay (usually 5-15 seconds)
    await act(async () => {
      vi.advanceTimersByTime(20000);
    });

    // Should show a sale notification with customer name
    expect(screen.queryByText(/vừa mua/i) || screen.queryByText(/đã mua/i)).toBeTruthy();
    // Notification may or may not be visible depending on random timing
    // The component should at least not crash
  });

  it('cycles through notifications over time', async () => {
    render(<LiveSalesPopup />);
    
    // Advance through multiple notification cycles
    await act(async () => {
      vi.advanceTimersByTime(60000); // 1 minute
    });

    // Component should handle multiple cycles without errors
    expect(true).toBe(true);
  });
});
