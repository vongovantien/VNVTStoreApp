import { test, expect } from '@playwright/test';

// Test Suite for Backend API - SEQUENTIAL because Login depends on Register
test.describe.serial('Backend API Tests', () => {

    const uniqueId = Math.random().toString(36).substring(7);
    const user = {
        username: `api_user_${uniqueId}`,
        email: `api_${uniqueId}@test.com`,
        password: 'Password123!',
        fullName: 'API Test User'
    };
    let authToken = '';

    test('Register User', async ({ request }) => {
        const response = await request.post('/api/v1/auth/register', {
            data: user
        });

        if (!response.ok()) {
            console.log('Register failed:', response.status(), await response.text());
        }
        expect(response.ok()).toBeTruthy();

        const body = await response.json();
        expect(body.success).toBe(true);
        // Depending on backend, username might be normalized or not.
        // Expect email mostly.
        expect(body.data.email).toBe(user.email);
    });

    test('Login User', async ({ request }) => {
        const response = await request.post('/api/v1/auth/login', {
            data: {
                username: user.username,
                password: user.password
            }
        });

        if (!response.ok()) {
            console.log('Login failed:', response.status(), await response.text());
        }
        expect(response.ok()).toBeTruthy();
        const body = await response.json();
        expect(body.success).toBe(true);
        expect(body.data.token).toBeDefined();

        authToken = body.data.token;
    });

    test('Get User Profile (Protected)', async ({ request }) => {
        // Route: /api/v1/users/profile
        const meResponse = await request.get('/api/v1/users/profile', {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (!meResponse.ok()) {
            console.log('Get Profile failed:', meResponse.status(), await meResponse.text());
        }
        expect(meResponse.ok()).toBeTruthy();
        const body = await meResponse.json();
        expect(body.success).toBe(true);
        expect(body.data.username).toBe(user.username.toLowerCase()); // assuming normalization or exact match
    });

    test('Public Products List', async ({ request }) => {
        const response = await request.post('/api/v1/Products/search', {
            data: {}
        });

        if (!response.ok()) {
            console.log('Search Products failed:', response.status(), await response.text());
        }
        expect(response.ok()).toBeTruthy();
        const body = await response.json();
        expect(body.success).toBe(true);
        expect(Array.isArray(body.data.items)).toBe(true);
    });

});
