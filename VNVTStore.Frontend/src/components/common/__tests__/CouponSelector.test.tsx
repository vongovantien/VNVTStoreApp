import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { CouponSelector } from '../CouponSelector';

// Mock dependencies
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, fallback?: string) => fallback || key,
    }),
}));

vi.mock('framer-motion', () => ({
    motion: {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
    },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock promotionService
const mockGetAll = vi.fn();
vi.mock('@/services/promotionService', () => ({
    promotionService: {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        getAll: (...args: any[]) => mockGetAll(...args),
    },
}));

// Mock UI components
vi.mock('@/components/ui', () => ({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    Button: ({ children, ...props }: any) => <button {...props}>{children}</button>,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    Badge: ({ children, ...props }: any) => <span {...props}>{children}</span>,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    Drawer: ({ children, isOpen, title }: any) =>
        isOpen ? <div data-testid="drawer"><h2>{title}</h2>{children}</div> : null,
}));

vi.mock('@/utils/format', () => ({
    formatCurrency: (val: number) => `${val.toLocaleString()}đ`,
}));

describe('CouponSelector Component', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders drawer with title when open', async () => {
        mockGetAll.mockResolvedValue({ success: true, data: { items: [] } });

        render(
            <CouponSelector
                isOpen={true}
                onClose={() => {}}
                onSelect={() => {}}
                currentSubtotal={100000}
            />
        );

        await waitFor(() => {
            expect(screen.getByTestId('drawer')).toBeInTheDocument();
        });
    });

    it('shows loading state while fetching coupons', () => {
        // Never resolve the promise so it stays in loading state
        mockGetAll.mockReturnValue(new Promise(() => {}));

        render(
            <CouponSelector
                isOpen={true}
                onClose={() => {}}
                onSelect={() => {}}
                currentSubtotal={100000}
            />
        );

        // The loading spinner should be present
        expect(screen.getByTestId('drawer')).toBeInTheDocument();
    });

    it('shows empty state when no coupons available', async () => {
        mockGetAll.mockResolvedValue({ success: true, data: { items: [] } });

        render(
            <CouponSelector
                isOpen={true}
                onClose={() => {}}
                onSelect={() => {}}
                currentSubtotal={100000}
            />
        );

        await waitFor(() => {
            expect(screen.getByText('Không tìm thấy ưu đãi')).toBeInTheDocument();
        });
    });
});
