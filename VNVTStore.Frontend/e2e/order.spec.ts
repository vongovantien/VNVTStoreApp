import { test, expect } from '@playwright/test';

test.describe('Shop Order Flow', () => {

    test.beforeEach(async ({ page }) => {
        // Shared start point: clear state fully
        await page.goto('/');
        await page.context().clearCookies();
        await page.evaluate(() => localStorage.clear());
        await page.evaluate(() => sessionStorage.clear());
    });

    test('should complete a GUEST purchase flow', async ({ page }) => {
        // 1. Add product to cart
        await page.goto('/products');
        const addToCartBtn = page.getByRole('button', { name: /add to cart|thêm vào giỏ/i }).first();
        await expect(addToCartBtn).toBeVisible();
        await addToCartBtn.click();

        await page.waitForTimeout(500);

        // 2. Go to Cart & Verify
        await page.goto('/cart');
        await expect(page.locator('h1')).toContainText(/Giỏ hàng|Cart/i);
        await expect(page.locator('.grid').first()).toBeVisible();

        // 3. Proceed to Checkout
        await page.getByRole('button', { name: /thanh toán|checkout/i }).click();

        // 4. Handle Guest Option in Modal
        const guestBtn = page.getByRole('button', { name: /mua hàng không cần đăng nhập/i });
        if (await guestBtn.isVisible({ timeout: 5000 })) {
            await guestBtn.click();
        }

        // 5. STEP 1: Shipping Info
        await expect(page.getByRole('heading', { name: /thông tin giao hàng/i })).toBeVisible();
        await page.getByPlaceholder('Nguyễn Văn A').fill('Guest User');
        await page.getByPlaceholder('0901234567').fill('0908887777');
        await page.getByPlaceholder('email@example.com').fill('guest@example.com');

        // Native select or custom components? Assuming standard interaction
        await page.locator('select').first().selectOption({ index: 1 });
        await page.locator('select').nth(1).selectOption({ index: 1 });
        await page.getByPlaceholder('Số nhà, tên đường...').fill('456 Guest Road');

        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 6. STEP 2: Payment Method
        await expect(page.getByRole('heading', { name: /phương thức thanh toán/i })).toBeVisible();
        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 7. STEP 3: Confirm Order
        await expect(page.getByRole('heading', { name: /xác nhận đơn hàng/i })).toBeVisible();
        const placeOrderBtn = page.getByRole('button', { name: /đặt hàng|place order/i });
        await expect(placeOrderBtn).toBeVisible();
        await placeOrderBtn.click();

        // 8. Final Success
        await expect(page).toHaveURL(/.*order-success/, { timeout: 15000 });
        await expect(page.getByText(/đặt hàng thành công/i)).toBeVisible();
    });

    test('should complete an AUTHENTICATED purchase flow', async ({ page }) => {
        // 1. First Login
        await page.goto('/login');
        await page.fill('[data-testid="email-input"]', 'admin@vnvtstore.com');
        await page.fill('[data-testid="password-input"]', 'Admin@123');
        await page.click('[data-testid="login-button"]');

        // Wait for login redirection
        await expect(page).toHaveURL(/\/admin|^\/$/); // Might redirect to admin or home depending on role/last page

        // 2. Add product to cart (go to shop)
        await page.goto('/products');
        const addToCartBtn = page.getByRole('button', { name: /add to cart|thêm vào giỏ/i }).first();
        await expect(addToCartBtn).toBeVisible();
        await addToCartBtn.click();

        // 3. Checkout
        await page.goto('/checkout');

        // 4. STEP 1: Shipping Info (Should be pre-filled or need filling)
        // If pre-filled, we check values, if not, we fill.
        const nameInput = page.getByPlaceholder('Nguyễn Văn A');
        if (!(await nameInput.inputValue())) {
            await nameInput.fill('Admin User');
            await page.getByPlaceholder('0901234567').fill('0901234567');
            await page.locator('select').first().selectOption({ index: 1 });
            await page.locator('select').nth(1).selectOption({ index: 1 });
            await page.getByPlaceholder('Số nhà, tên đường...').fill('123 Admin St');
        }

        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 5. STEP 2: Payment
        await expect(page.getByRole('heading', { name: /phương thức thanh toán/i })).toBeVisible();
        await page.getByRole('button', { name: /tiếp tục|continue/i }).click();

        // 6. STEP 3: Confirm
        await expect(page.getByRole('heading', { name: /xác nhận đơn hàng/i })).toBeVisible();
        await page.getByRole('button', { name: /đặt hàng|place order/i }).click();

        // 7. Success
        await expect(page).toHaveURL(/.*order-success/);
        await expect(page.getByText(/đặt hàng thành công/i)).toBeVisible();
    });
});
