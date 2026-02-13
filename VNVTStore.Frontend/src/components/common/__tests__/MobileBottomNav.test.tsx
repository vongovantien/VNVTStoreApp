/**
 * Unit tests for MobileBottomNav component
 * Features: #72 Mobile Bottom Navigation Bar, #74 Haptic Feedback
 */
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
    nav: ({ children, ...props }: any) => <nav {...props}>{children}</nav>,
    span: ({ children, ...props }: any) => <span {...props}>{children}</span>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useLocation: vi.fn(() => ({ pathname: '/' })),
  useNavigate: vi.fn(() => mockNavigate),
}));

// Mock stores - items.length is used for badge count
vi.mock('@/store', () => ({
  useCartStore: vi.fn((selector: any) => selector({ items: [{ code: 'P1' }, { code: 'P2' }] })),
  useWishlistStore: vi.fn((selector: any) => selector({ items: [] })),
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Home: (props: any) => <span {...props}>Home</span>,
  Search: (props: any) => <span {...props}>Search</span>,
  ShoppingCart: (props: any) => <span {...props}>Cart</span>,
  Heart: (props: any) => <span {...props}>Heart</span>,
  User: (props: any) => <span {...props}>User</span>,
  Menu: (props: any) => <span {...props}>Menu</span>,
  Grid3x3: (props: any) => <span {...props}>Grid</span>,
}));

import { MobileBottomNav } from '../MobileBottomNav';

describe('MobileBottomNav', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // Feature #72: Bottom Navigation Bar
  describe('#72 Mobile Bottom Navigation', () => {
    it('renders navigation buttons', () => {
      render(<MobileBottomNav />);
      const buttons = screen.getAllByRole('button');
      expect(buttons.length).toBeGreaterThan(0);
    });

    it('shows cart badge with item count', () => {
      render(<MobileBottomNav />);
      // Cart should show count — items.length = 2
      expect(screen.getByText('2')).toBeInTheDocument();
    });

    it('navigates on button click', () => {
      render(<MobileBottomNav />);
      const buttons = screen.getAllByRole('button');
      fireEvent.click(buttons[0]);
      expect(mockNavigate).toHaveBeenCalled();
    });
  });

  // Feature #74: Haptic Feedback
  describe('#74 Haptic Feedback', () => {
    it('triggers vibration on tap when supported', () => {
      const mockVibrate = vi.fn();
      Object.defineProperty(navigator, 'vibrate', { value: mockVibrate, writable: true, configurable: true });
      
      render(<MobileBottomNav />);
      const buttons = screen.getAllByRole('button');
      fireEvent.click(buttons[0]);
      expect(mockVibrate).toHaveBeenCalledWith(10);
    });

    it('does not crash when vibration is not supported', () => {
      Object.defineProperty(navigator, 'vibrate', { value: undefined, writable: true, configurable: true });
      
      render(<MobileBottomNav />);
      const buttons = screen.getAllByRole('button');
      expect(() => {
        fireEvent.click(buttons[0]);
      }).not.toThrow();
    });
  });
});
