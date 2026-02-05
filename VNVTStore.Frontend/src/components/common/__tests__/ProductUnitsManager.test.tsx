import { render, screen, within } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import '@testing-library/jest-dom/vitest';
import { ProductUnitsManager, ProductUnitDto } from '../ProductUnitsManager';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// Mock dependencies
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: Record<string, unknown>) => {
        if (key === 'messages.confirmDelete') return `Are you sure you want to delete ${options?.name}?`;
        return key;
    },
  }),
}));

// Mock ConfirmDialog and Badge to simplify testing
vi.mock('@/components/ui', async (importOriginal) => {
    const actual = await importOriginal<Record<string, unknown>>();
    return {
        ...actual,
        ConfirmDialog: ({ isOpen, onConfirm, message }: { isOpen: boolean; onConfirm: () => void; message: string }) => {
            if (!isOpen) return null;
            return (
                <div data-testid="confirm-dialog">
                    <span>{message}</span>
                    <button onClick={onConfirm}>ConfirmDelete</button>
                </div>
            );
        },
        Badge: ({ children }: { children: React.ReactNode }) => <div data-testid="mock-badge">{children}</div>
    };
});

describe('ProductUnitsManager Component', () => {

    const baseProps = {
        baseUnitName: 'Cái',
        baseUnitPrice: 100000,
        onChange: vi.fn(),
        fetchUnitOptions: vi.fn().mockResolvedValue({ items: [], totalItems: 0 }),
        units: [] as ProductUnitDto[]
    };

    const renderWithProvider = (props = baseProps) => {
        const queryClient = new QueryClient({
            defaultOptions: {
                queries: { retry: false },
            },
        });
        return render(
            <QueryClientProvider client={queryClient}>
                <ProductUnitsManager {...props} />
            </QueryClientProvider>
        );
    };

    it('renders base unit correctly', () => {
        renderWithProvider();
        
        const elements = screen.getAllByText('Cái');
        expect(elements.length).toBeGreaterThan(0);
        expect(elements[0]).toBeInTheDocument();
        // Check for mock badge
        expect(screen.getByTestId('mock-badge')).toBeInTheDocument();
        expect(screen.getByText('product.defaultUnit')).toBeInTheDocument();
        // Use regex to match price regardless of locale separator
        expect(screen.getByText(/100[.,]000/)).toBeInTheDocument(); 
    });

    it('renders additional units', async () => {
        const units: ProductUnitDto[] = [
            { code: 'U1', unitName: 'Hộp', conversionRate: 10, price: 950000, isActive: true, isBaseUnit: false }
        ];
        renderWithProvider({ ...baseProps, units });
        
        // Use getAllByText if multiples expected, or better yet, be specific
        const unitRows = screen.getAllByTestId(/unit-row-/);
        expect(unitRows).toHaveLength(1);
        
        expect(within(unitRows[0]).getByText(/Hộp/i)).toBeInTheDocument();
        expect(within(unitRows[0]).getByDisplayValue('10')).toBeInTheDocument();
        expect(within(unitRows[0]).getByText(new RegExp(`× ${baseProps.baseUnitName}`, 'i'))).toBeInTheDocument();
        expect(within(unitRows[0]).getByDisplayValue('950.000')).toBeInTheDocument();
    });

    it('calls onChange when add button is clicked', async () => {
        renderWithProvider();
        
        const addButton = screen.getByText(/admin\.actions\.addUnit/i);
        await userEvent.click(addButton);
        
        expect(baseProps.onChange).toHaveBeenCalled();
    });

    it('calls onChange when delete is clicked', async () => {
        const units: ProductUnitDto[] = [
            { code: 'U1', unitName: 'Hộp', conversionRate: 10, price: 950000, isActive: true, isBaseUnit: false }
        ];
        renderWithProvider({ ...baseProps, units });
        
        // Find delete button by testid
        const deleteButton = screen.getByTestId('delete-unit-button-0');
        await userEvent.click(deleteButton); 
        
        // Check for mocked dialog
        const dialog = await screen.findByTestId('confirm-dialog');
        expect(dialog).toBeInTheDocument();
        expect(within(dialog).getByText(/Hộp/i)).toBeInTheDocument();
        
        const confirmBtn = screen.getByText('ConfirmDelete');
        await userEvent.click(confirmBtn);
        
        expect(baseProps.onChange).toHaveBeenCalled();
    });
});
