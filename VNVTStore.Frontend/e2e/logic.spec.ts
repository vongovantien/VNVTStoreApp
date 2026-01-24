import { test, expect } from '@playwright/test';

test.describe('Business Logic Verification', () => {
    test.beforeEach(async ({ page }) => {
        await page.context().clearCookies();
    });

    test('should calculate cart totals and shipping fee correctly', async ({ page }) => {
        // 1. Add item to cart
        await page.goto('/products');

        // We need a product with known price or just dynamic check.
        // Let's grab the first product price from the card
        const firstProductCard = page.locator('.group').first();
        // Assuming grid layout default
        const priceText = await firstProductCard.locator('.text-error').first().innerText();
        const cleanPrice = parseInt(priceText.replace(/[^\d]/g, ''));

        await firstProductCard.getByRole('button', { name: /add to cart|thêm vào giỏ/i }).click();
        await page.waitForTimeout(500);

        // 2. Go to Cart
        await page.goto('/cart');

        // 3. Verify Subtotal
        // Cart page has subtotal in summary
        const subtotalEl = page.locator('.bg-primary').getByText('Tạm tính').locator('..').locator('span').last();
        // Or closer selector: .bg-primary -> contains text 'Tạm tính' -> sibling span
        // In CartPage.tsx: <div className="flex justify-between text-secondary"><span>{t('cart.subtotal')}</span><span>{formatCurrency(total)}</span></div>

        // Let's try text matching
        const subtotalText = await subtotalEl.innerText();
        const cartSubtotal = parseInt(subtotalText.replace(/[^\d]/g, ''));

        expect(cartSubtotal).toBe(cleanPrice);

        // 4. Verify Shipping Fee Logic
        // Fee is 30k if < 500k, 0 if >= 500k
        const shippingEl = page.locator('.bg-primary').getByText('Phí vận chuyển').locator('..').locator('span').last();
        let shippingText = await shippingEl.innerText();

        let expectedShipping = cartSubtotal >= 500000 ? 0 : 30000;

        if (expectedShipping === 0) {
            // Might say "Miễn phí"
            expect(shippingText).toMatch(/miễn phí|0/i);
        } else {
            const shippingFee = parseInt(shippingText.replace(/[^\d]/g, ''));
            expect(shippingFee).toBe(30000);
        }

        // 5. Update Quantity to trigger Free Shipping (if not already)
        // If current total < 500k, increase quantity until > 500k
        if (cartSubtotal < 500000) {
            const requiredQty = Math.ceil(500000 / cleanPrice);
            // Find quantity input or plus button
            // In CartPage.tsx: plus button has <Plus size={16} /> inside a button
            // <button onClick={() => updateQuantity...}><Plus../></button>
            const plusBtn = page.locator('button:has(svg.lucide-plus)');

            // Click (requiredQty - 1) times
            for (let i = 0; i < requiredQty - 1; i++) {
                await plusBtn.click();
                await page.waitForTimeout(200); // Wait for state update
            }

            // Re-check shipping
            shippingText = await shippingEl.innerText();
            expect(shippingText).toMatch(/miễn phí|0/i);
        }
    });
});
