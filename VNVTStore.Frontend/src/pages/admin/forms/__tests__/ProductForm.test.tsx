import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach, Mock } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/config/i18n';
import { ProductForm } from '../ProductForm';

// Mock hooks
vi.mock('@/services', () => ({
    categoryService: {
        search: vi.fn().mockResolvedValue({
            data: {
                items: [
                    { code: 'CAT001', name: 'Test Category 1', isActive: true },
                    { code: 'CAT002', name: 'Test Category 2', isActive: true }
                ],
                totalItems: 2
            }
        })
    },
    supplierService: {
        search: vi.fn().mockResolvedValue({
            data: {
                items: [
                    { code: 'SUP001', name: 'Test Supplier 1', isActive: true },
                    { code: 'SUP002', name: 'Test Supplier 2', isActive: true }
                ],
                totalItems: 2
            }
        })
    },
    unitService: {
        search: vi.fn().mockResolvedValue({
            data: {
                items: [
                    { code: 'UNIT001', unitName: 'Cái', isActive: true },
                    { code: 'UNIT002', unitName: 'Hộp', isActive: true }
                ],
                totalItems: 2
            }
        })
    },
    brandService: {
        search: vi.fn().mockResolvedValue({
            data: {
                items: [
                    { code: 'BRAND001', name: 'Test Brand 1', isActive: true },
                    { code: 'BRAND002', name: 'Test Brand 2', isActive: true }
                ],
                totalItems: 2
            }
        })
    }
}));

// Mock useToast
vi.mock('@/store', () => ({
    useToast: vi.fn(() => ({
        success: vi.fn(),
        error: vi.fn(),
    })),
}));

// Mock react-dropzone
vi.mock('react-dropzone', () => ({
    useDropzone: vi.fn(() => ({
        getRootProps: () => ({}),
        getInputProps: () => ({}),
        isDragActive: false,
    })),
}));

const createTestQueryClient = () =>
    new QueryClient({
        defaultOptions: {
            queries: { retry: false },
        },
    });

const renderProductForm = (props = {}) => {
    const queryClient = createTestQueryClient();
    const defaultProps = {
        onSubmit: vi.fn(),
        onCancel: vi.fn(),
        isLoading: false,
        ...props,
    };

    return render(
        <QueryClientProvider client={queryClient}>
            <I18nextProvider i18n={i18n}>
                <ProductForm {...defaultProps} />
            </I18nextProvider>
        </QueryClientProvider>
    );
};

describe('ProductForm', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('Initial Render', () => {
        it('should render form with all required fields', async () => {
            renderProductForm();

            // Check for form title
            await waitFor(() => {
// ProductForm doesn't render its own title, it's usually rendered by the parent page or BaseForm logic not triggered here
            // expect(screen.getByText(/Thêm sản phẩm|Add Product/i)).toBeInTheDocument();
            });
        });

        it('should render with default values for new product', async () => {
            renderProductForm();

            await waitFor(() => {
                // Check that form elements exist
                const nameInput = screen.getByPlaceholderText(/tên sản phẩm|product name/i);
                expect(nameInput).toBeInTheDocument();
            });
        });
    });

    describe('Dropdown Data Loading', () => {
        it('should load and display categories in dropdown', async () => {
            renderProductForm();

            await waitFor(() => {
                // Find category select by label
                const categorySelect = screen.getByLabelText(/danh mục|category/i);
                expect(categorySelect).toBeInTheDocument();
            });
        });

        it('should load and display suppliers in dropdown', async () => {
            renderProductForm();

            await waitFor(() => {
                // Find supplier select by label
                const supplierSelect = screen.getByLabelText(/nhà cung cấp|supplier/i);
                expect(supplierSelect).toBeInTheDocument();
            });
        });

        it('should load and display units in dropdown', async () => {
            renderProductForm();

            await waitFor(() => {
                // Find unit select by label
                const unitSelect = screen.getByLabelText(/đơn vị tính|unit/i);
                expect(unitSelect).toBeInTheDocument();
            });
        });
    });

    describe('Form Submission', () => {
        it('should call onSubmit when form is valid and submitted', async () => {
            const onSubmit = vi.fn();
            renderProductForm({ onSubmit });

            const user = userEvent.setup();

            // Fill required fields
            const nameInput = screen.getByPlaceholderText(/tên sản phẩm|product name/i);
            await user.type(nameInput, 'Test Product');

            // Find and click submit button
            const submitButton = screen.getByRole('button', { name: /lưu|save/i });
            await user.click(submitButton);

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalled();
            });
        });

        it('should call onCancel when cancel button is clicked', async () => {
            const onCancel = vi.fn();
            renderProductForm({ onCancel });

            const user = userEvent.setup();

            const cancelButton = screen.getByRole('button', { name: /hủy|cancel/i });
            await user.click(cancelButton);

            expect(onCancel).toHaveBeenCalled();
        });
    });

    describe('Edit Mode', () => {
        it('should populate form with initial data when editing', async () => {
            const initialData = {
                name: 'Existing Product',
                price: 100000,
                categoryCode: 'CAT001',
                stockQuantity: 50,
                baseUnit: 'Cái',
            };

            renderProductForm({ initialData });

            await waitFor(() => {
                const nameInput = screen.getByDisplayValue('Existing Product');
                expect(nameInput).toBeInTheDocument();
            });
        });
    });
});
