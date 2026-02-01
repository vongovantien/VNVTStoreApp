import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AppRouter } from './router';
import { ToastContainer } from '@/components/ui/Toast';
import { ConfirmProvider } from '@/context/ConfirmContext';
import './config/i18n';

// Create query client with caching config
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfirmProvider>
        <AppRouter />
        <ToastContainer />
      </ConfirmProvider>
    </QueryClientProvider>
  );
}

export default App;
