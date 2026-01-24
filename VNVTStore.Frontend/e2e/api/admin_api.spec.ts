import { test, expect } from '@playwright/test';

test.describe.serial('Admin API Verification', () => {
    let authToken = '';

    test('Login as Admin', async ({ request }) => {
        const response = await request.post('/api/v1/auth/login', {
            data: {
                username: 'admin',
                password: 'Password123!'
            }
        });

        if (!response.ok()) {
            console.log('Admin Login failed:', response.status(), await response.text());
        }
        expect(response.ok()).toBeTruthy();
        const body = await response.json();
        authToken = body.data.token;
        expect(authToken).toBeDefined();
    });

    test('Get Suppliers', async ({ request }) => {
        const response = await request.get('/api/v1/suppliers', {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        if (!response.ok()) console.log('Get Suppliers failed:', await response.text());
        expect(response.ok()).toBeTruthy();
    });

    test('Get Categories', async ({ request }) => {
        // CategoriesController inherits BaseApiController which has [HttpPost("search")]
        // GET /api/v1/categories might not exist if not explicitly added.
        const response = await request.post('/api/v1/categories/search', {
            headers: { 'Authorization': `Bearer ${authToken}` },
            data: {}
        });
        if (!response.ok()) console.log('Get Categories failed:', await response.text());
        expect(response.ok()).toBeTruthy();
    });

    test('Get Products', async ({ request }) => {
        // Products likely similar: POST /search
        const response = await request.post('/api/v1/products/search', {
            headers: { 'Authorization': `Bearer ${authToken}` },
            data: {}
        });
        if (!response.ok()) console.log('Get Products failed:', await response.text());
        expect(response.ok()).toBeTruthy();
    });

    test('Get Orders', async ({ request }) => {
        // Orders likely similar: POST /search
        const response = await request.post('/api/v1/orders/search', {
            headers: { 'Authorization': `Bearer ${authToken}` },
            data: {}
        });
        if (!response.ok()) console.log('Get Orders failed:', await response.text());
        expect(response.ok()).toBeTruthy();
    });
});
