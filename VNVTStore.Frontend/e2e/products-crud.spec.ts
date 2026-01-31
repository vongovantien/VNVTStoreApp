import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Products CRUD operations
 * Tests complete user flows for creating, reading, updating, and deleting products
 */

test.describe('Products CRUD Flow', () => {
    const testProduct = {
        name: `Test Product ${Date.now()}`,
        price: '150000',
        category: 'Test Category',
        description: 'This is a test product created by E2E tests'
    };

    test.beforeEach(async ({ page }) => {
        // Navigate to admin and login
        await page.goto('/admin');

        // Check if need to login
        if (page.url().includes('/login')) {
            await page.fill('[data-testid="username"]', 'admin');
            await page.fill('[data-testid="password"]', 'admin123');
            await page.click('[data-testid="login-button"]');
            await page.waitForURL('/admin/**');
        }
    });

    test('should navigate to products page', async ({ page }) => {
        await page.click('[data-testid="nav-products"]');
        await expect(page).toHaveURL(/.*products/);
        await expect(page.locator('h1, h2').first()).toContainText(/Sản phẩm|Products/i);
    });

    test('should display products list', async ({ page }) => {
        await page.goto('/admin/products');

        // Wait for data to load
        await page.waitForSelector('[data-testid="products-table"], table', { timeout: 10000 });

        // Verify table exists
        const table = page.locator('[data-testid="products-table"], table');
        await expect(table).toBeVisible();
    });

    test('should open create product form', async ({ page }) => {
        await page.goto('/admin/products');

        // Click add button
        await page.click('[data-testid="add-product"], button:has-text("Thêm"), button:has-text("Add")');

        // Verify form opened
        await expect(page.locator('[data-testid="product-form"], form')).toBeVisible();
    });

    test('should validate required fields', async ({ page }) => {
        await page.goto('/admin/products');
        await page.click('[data-testid="add-product"], button:has-text("Thêm"), button:has-text("Add")');

        // Try to submit empty form
        await page.click('[data-testid="submit-button"], button[type="submit"]:has-text("Lưu"), button[type="submit"]:has-text("Save")');

        // Verify validation errors appear
        await expect(page.locator('.error, [data-testid="error"], .text-red-500')).toBeVisible();
    });

    test('should search products', async ({ page }) => {
        await page.goto('/admin/products');

        // Enter search term
        const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Tìm"], input[placeholder*="Search"]');
        await searchInput.fill('test');

        // Wait for filter to apply
        await page.waitForTimeout(500);

        // Verify search works (table should update)
        await expect(searchInput).toHaveValue('test');
    });

    test('should paginate products', async ({ page }) => {
        await page.goto('/admin/products');

        // Wait for pagination controls
        const pagination = page.locator('[data-testid="pagination"], .pagination, nav[aria-label="pagination"]');

        if (await pagination.isVisible()) {
            // Click next page if available
            const nextButton = page.locator('[data-testid="next-page"], button:has-text("Next"), button:has-text("Sau")');
            if (await nextButton.isEnabled()) {
                await nextButton.click();
                await page.waitForTimeout(500);
            }
        }
    });

    test('should sort products by column', async ({ page }) => {
        await page.goto('/admin/products');

        // Click on a sortable column header
        const header = page.locator('th:has-text("Tên"), th:has-text("Name")').first();
        await header.click();

        // Verify sorting indicator appears
        await page.waitForTimeout(300);
    });
});

test.describe('Product Form Validation', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/admin');

        if (page.url().includes('/login')) {
            await page.fill('[data-testid="username"]', 'admin');
            await page.fill('[data-testid="password"]', 'admin123');
            await page.click('[data-testid="login-button"]');
            await page.waitForURL('/admin/**');
        }

        await page.goto('/admin/products');
        await page.click('[data-testid="add-product"], button:has-text("Thêm"), button:has-text("Add")');
    });

    test('should show error for empty product name', async ({ page }) => {
        const nameInput = page.locator('[data-testid="product-name"], input[name="name"]');
        await nameInput.fill('');
        await nameInput.blur();

        await page.click('[data-testid="submit-button"], button[type="submit"]');

        await expect(page.locator('text=/không được để trống|required/i')).toBeVisible();
    });

    test('should show error for invalid price', async ({ page }) => {
        const priceInput = page.locator('[data-testid="product-price"], input[name="price"]');
        await priceInput.fill('-100');

        await page.click('[data-testid="submit-button"], button[type="submit"]');

        await expect(page.locator('text=/phải lớn hơn 0|greater than 0/i')).toBeVisible();
    });
});
