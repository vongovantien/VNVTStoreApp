import { test, expect } from '@playwright/test';

test.describe('Comprehensive Shopping Flow', () => {

    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.context().clearCookies();
        await page.evaluate(() => localStorage.clear());
    });

    test('should complete a Full Shopping Flow (Search -> Cart -> Checkout)', async ({ page }) => {
        // 1. Search for a product
        await page.goto('/');
        const searchInput = page.getByPlaceholder(/Tìm kiếm sản phẩm/i);
        await searchInput.fill('iphone');
        await searchInput.press('Enter');

        // 2. Select first product from results
        await page.waitForURL(/.*search.*/);
        const firstProduct = page.locator('.product-card').first();
        await expect(firstProduct).toBeVisible();
        await firstProduct.click();

        // 3. Add to Cart on Detail Page
        await page.waitForURL(/.*product.*/);
        const addToCartBtn = page.getByRole('button', { name: /thêm vào giỏ/i });
        await expect(addToCartBtn).toBeVisible();
        await addToCartBtn.click();

        // 4. View Cart
        const cartIcon = page.locator('.cart-icon-wrapper'); // Adjust selector as needed
        await cartIcon.click();
        await expect(page).toHaveURL(/.*cart/);

        // 5. Apply Coupon (if selector exists)
        const openCouponBtn = page.getByRole('button', { name: /chọn mã giảm giá/i });
        if (await openCouponBtn.isVisible()) {
            await openCouponBtn.click();
            const firstCoupon = page.locator('.coupon-item').first();
            if (await firstCoupon.isVisible()) {
                await firstCoupon.click();
            }
        }

        // 6. Proceed to Checkout
        await page.getByRole('button', { name: /thanh toán/i }).click();

        // 7. Complete Checkout as Guest
        const guestBtn = page.getByRole('button', { name: /mua hàng không cần đăng nhập/i });
        if (await guestBtn.isVisible()) {
            await guestBtn.click();
        }

        await page.getByPlaceholder('Nguyễn Văn A').fill('E2E Tester');
        await page.getByPlaceholder('0901234567').fill('0908889999');
        await page.getByPlaceholder('email@example.com').fill('e2e@test.com');

        // Select random city/district
        await page.locator('select').first().selectOption({ index: 1 });
        await page.locator('select').nth(1).selectOption({ index: 1 });
        await page.getByPlaceholder('Số nhà, tên đường...').fill('123 Selenium Road');

        await page.getByRole('button', { name: /tiếp tục/i }).click();
        await page.getByRole('button', { name: /tiếp tục/i }).click(); // Payment step

        const placeOrderBtn = page.getByRole('button', { name: /đặt hàng/i });
        await expect(placeOrderBtn).toBeVisible();
        await placeOrderBtn.click();

        // 8. Success Check
        await expect(page).toHaveURL(/.*order-success/, { timeout: 15000 });
        await expect(page.getByText(/đặt hàng thành công/i)).toBeVisible();
    });

    test('should complete "Buy Now" flow directly', async ({ page }) => {
        // 1. Go to a specific product
        await page.goto('/products');
        const firstProductLink = page.locator('a[href*="/product/"]').first();
        await firstProductLink.click();

        // 2. Click Buy Now
        const buyNowBtn = page.getByRole('button', { name: /mua ngay|buy now/i });
        await expect(buyNowBtn).toBeVisible();
        await buyNowBtn.click();

        // 3. Verify Direct Checkout Modal/Page
        // Assuming "Buy Now" opens a direct modal or instant checkout
        await expect(page.getByText(/thanh toán nhanh|direct checkout/i).or(page.getByText(/thông tin giao hàng/i))).toBeVisible();

        // 4. Fill and Complete (Shortened if modal, otherwise same as checkout)
        const nameInput = page.getByPlaceholder('Nguyễn Văn A');
        await nameInput.fill('Buy Now Tester');
        await page.getByPlaceholder('0901234567').fill('0911222333');
        await page.locator('select').first().selectOption({ index: 1 });
        await page.locator('select').nth(1).selectOption({ index: 1 });
        await page.getByPlaceholder('Số nhà, tên đường...').fill('789 Instant St');

        await page.getByRole('button', { name: /đặt hàng|xác nhận/i }).first().click();

        // 5. Verify Success
        await expect(page).toHaveURL(/.*order-success/);
    });
});
