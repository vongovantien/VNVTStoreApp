import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// Import translations
import viTranslation from '@/locales/vi.json';
import enTranslation from '@/locales/en.json';

const resources = {
    vi: { translation: viTranslation },
    en: { translation: enTranslation },
};

i18n.use(initReactI18next).init({
    resources,
    lng: localStorage.getItem('language') || 'vi',
    fallbackLng: 'vi',
    interpolation: {
        escapeValue: false,
    },
    react: {
        useSuspense: true,
    },
});

export default i18n;
