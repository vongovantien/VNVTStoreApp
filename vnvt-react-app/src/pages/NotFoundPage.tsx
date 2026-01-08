import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Home, HelpCircle } from 'lucide-react';

export const NotFoundPage = (): JSX.Element => {
  const { t } = useTranslation();

  return (
    <main className="grid min-h-screen place-items-center bg-primary px-6 py-24 sm:py-32 lg:px-8">
      <div className="text-center">
        <p className="text-base font-semibold text-indigo-600 dark:text-indigo-400">404</p>
        <h1 className="mt-4 text-5xl font-semibold tracking-tight text-balance text-primary sm:text-7xl">
          {t('errors.pageNotFound', 'Page not found')}
        </h1>
        <p className="mt-6 text-lg font-medium text-pretty text-secondary sm:text-xl">
          {t('errors.pageNotFoundDescription', "Sorry, we couldn't find the page you're looking for.")}
        </p>
        <div className="mt-10 flex items-center justify-center gap-x-6">
          <Link
            to="/"
            className="inline-flex items-center gap-2 rounded-md bg-indigo-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 transition-colors"
          >
            <Home size={18} />
            {t('common.goHome', 'Go back home')}
          </Link>
          <Link
            to="/contact"
            className="inline-flex items-center gap-1 text-sm font-semibold text-primary hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
          >
            <HelpCircle size={16} />
            {t('common.contactSupport', 'Contact support')}
            <span aria-hidden="true">&rarr;</span>
          </Link>
        </div>
      </div>
    </main>
  );
};

export default NotFoundPage;
