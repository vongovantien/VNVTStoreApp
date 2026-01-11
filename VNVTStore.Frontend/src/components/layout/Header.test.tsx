import { screen, fireEvent, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../test/test-utils';
import { describe, it, expect, vi } from 'vitest';
import { Header } from './Header';

// Mocks
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string) => key,
        i18n: {
            language: 'vi',
            changeLanguage: vi.fn(),
        },
    }),
}));

// Mock stores
vi.mock('@/store', () => ({
    useCartStore: () => ({ getItemCount: () => 0 }),
    useWishlistStore: () => ({ items: [] }),
    useCompareStore: () => ({ items: [] }),
    useUIStore: () => ({
        theme: 'light',
        toggleTheme: vi.fn(),
        setCartOpen: vi.fn(),
    }),
}));

// Mock ResizeObserver for framer-motion
global.ResizeObserver = vi.fn().mockImplementation(() => ({
    observe: vi.fn(),
    unobserve: vi.fn(),
    disconnect: vi.fn(),
}));

describe('Header Component', () => {

    it('renders language switcher correctly', () => {
        renderWithProviders(<Header />);

        // Check for Language button (displaying VI)
        const langBtn = screen.getByText('vi');
        expect(langBtn).toBeInTheDocument();
    });

    it('opens language menu on click', async () => {
        renderWithProviders(<Header />);

        const langBtn = screen.getByText('vi').closest('button');
        fireEvent.click(langBtn!);

        // Expect dropdown options to appear
        await waitFor(() => {
            expect(screen.getByText('🇻🇳 Tiếng Việt')).toBeInTheDocument();
            expect(screen.getByText('🇺🇸 English')).toBeInTheDocument();
        });
    });
});
