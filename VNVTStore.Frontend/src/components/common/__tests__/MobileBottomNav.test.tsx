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
    div: ({ children, ...props }: { children: React.ReactNode }) => <div {...props}>{children}</div>,
    nav: ({ children, ...props }: { children: React.ReactNode }) => <nav {...props}>{children}</nav>,
    span: ({ children, ...props }: { children: React.ReactNode }) => <span {...props}>{children}</span>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useLocation: vi.fn(() => ({ pathname: '/' })),
  useNavigate: vi.fn(() => mockNavigate),
}));

// Mock stores - items.length is used for badge count
vi.mock('@/store', () => ({
  useCartStore: vi.fn((selector: (state: { items: { code: string }[] }) => unknown) => selector({ items: [{ code: 'P1' }, { code: 'P2' }] })),
  useWishlistStore: vi.fn((selector: (state: { items: { code: string }[] }) => unknown) => selector({ items: [] })),
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Home: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Home</span>,
  Search: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Search</span>,
  ShoppingCart: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Cart</span>,
  Heart: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Heart</span>,
  User: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>User</span>,
  Menu: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Menu</span>,
  Grid3x3: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Grid</span>,
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
