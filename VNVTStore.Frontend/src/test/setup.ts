
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';


// Runs a cleanup after each test case (e.g. clearing jsdom)
afterEach(() => {
    cleanup();

});

// Mock react-i18next
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, secondArg?: unknown) => {
            if (typeof secondArg === 'string') return secondArg;
            if (secondArg && typeof secondArg === 'object' && 'defaultValue' in secondArg) {
                return (secondArg as { defaultValue: string }).defaultValue;
            }
            return key;
        },
        i18n: {
            changeLanguage: vi.fn(),
            language: 'en',
        },
    }),
    initReactI18next: {
        type: '3rdParty',
        init: vi.fn(),
    },
    I18nextProvider: ({ children }: { children: React.ReactNode }) => children,
    Trans: ({ children }: { children: React.ReactNode }) => children,
    Translation: ({ children }: { children: (t: (key: string) => string, { i18n }: { i18n: unknown }) => React.ReactNode }) =>
        children((key: string) => key, { i18n: {} }),
}));
