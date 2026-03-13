import { createRoot } from 'react-dom/client';
import App from './App';
// import { MinimalApp } from './MinimalApp';
import './index.css';
import { injectStore } from './services/api';
import { useAuthStore } from './store';
import { initializeStoreEffects } from './store/effects';
import { registerSW } from 'virtual:pwa-register';

// Register service worker
registerSW({ immediate: true });

// Inject store to avoid circular dependency
injectStore(useAuthStore);

// Initialize store side effects (sync logic)
initializeStoreEffects();

createRoot(document.getElementById('root')!).render(
  <App />
);
