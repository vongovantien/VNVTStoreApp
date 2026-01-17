import { createRoot } from 'react-dom/client';
import App from './App';
import './index.css';
import { injectStore } from './services/api';
import { useAuthStore } from './store';

// Inject store to avoid circular dependency
injectStore(useAuthStore);

createRoot(document.getElementById('root')!).render(
  <App />
);
