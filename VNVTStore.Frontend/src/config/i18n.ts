import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// Import translations
import viTranslation from '@/locales/vi.json';
import enTranslation from '@/locales/en.json';
import sharedTranslation from '@/locales/shared.json';

const resources = {
    vi: {
        translation: { ...viTranslation, shared: { ...viTranslation.shared, ...sharedTranslation } }
    },
    en: {
        translation: { ...enTranslation, shared: { ...enTranslation.shared, ...sharedTranslation } }
    },
};

// Supported languages in this app
const SUPPORTED_LANGUAGES = Object.keys(resources); // ['vi', 'en']
const DEFAULT_LANGUAGE = 'vi';

/**
 * Detect the best language based on:
 * 1. User's saved preference in localStorage
 * 2. Browser's language (navigator.language / navigator.languages)
 *    - This reflects the user's OS locale & country settings
 *    - e.g., a user in Vietnam → 'vi-VN', in US → 'en-US', in Japan → 'ja-JP'
 * 3. Fallback to default ('vi')
 */
function detectLanguage(): string {
    // 1. Check saved preference
    const saved = localStorage.getItem('language');
    if (saved && SUPPORTED_LANGUAGES.includes(saved)) {
        return saved;
    }

    // 2. Detect from browser locale (navigator.languages has all preferred, navigator.language has primary)
    const browserLanguages = navigator.languages?.length
        ? [...navigator.languages]
        : [navigator.language];

    for (const lang of browserLanguages) {
        // Try exact match first (e.g., 'vi', 'en')
        if (SUPPORTED_LANGUAGES.includes(lang)) {
            return lang;
        }
        // Try base language (e.g., 'en-US' → 'en', 'vi-VN' → 'vi')
        const baseLang = lang.split('-')[0];
        if (SUPPORTED_LANGUAGES.includes(baseLang)) {
            return baseLang;
        }
    }

    // 3. Fallback
    return DEFAULT_LANGUAGE;
}

const detectedLng = detectLanguage();

i18n.use(initReactI18next).init({
    resources,
    lng: detectedLng,
    fallbackLng: DEFAULT_LANGUAGE,
    interpolation: {
        escapeValue: false,
    },
    react: {
        useSuspense: true,
    },
});

export default i18n;

