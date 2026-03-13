import { useState, useEffect } from 'react';
// Removed unused translation import
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { ShieldAlert } from 'lucide-react';
import { AppRouter } from './router';
import { ToastContainer } from '@/components/ui/Toast';
import { ConfirmProvider } from '@/context/ConfirmContext';
import './config/i18n';

import { ServiceProvider } from '@/core/di/ServiceContext';
import { useAuthStore, useToastStore } from '@/store';
import { signalRService } from '@/services/signalrService';
import { BuyNowModal } from '@/features/checkout/components/BuyNowModal';
import { CheckoutService } from './features/checkout/services/CheckoutService';
import { CheckoutRepository } from './features/checkout/repositories/CheckoutRepository';
import { PriceHistoryService } from './services/product/priceHistory/PriceHistoryService';
import { ServiceKeys } from './core/di/ServiceKeys';

// Initialize Services
const services = {
  [ServiceKeys.Checkout]: new CheckoutService(new CheckoutRepository()),
  [ServiceKeys.PriceHistory]: new PriceHistoryService(),
};

// Create query client with caching config
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 1000 * 60 * 5,
    },
  },
});



function App() {
  const { token, isAuthenticated } = useAuthStore();
  const { success, error, info } = useToastStore();
  
  const [isMaintenance] = useState(() => 
    window.location.search.includes('vnvt_os_state=maintenance')
  );

  // Maintenance state is now initialized via lazy initializer in useState
  // Feature: SignalR Notification System
  useEffect(() => {
    const handleNotification = (data: unknown) => {
        if (typeof data === 'string') {
             info(data, 5000);
        } else if (data && typeof data === 'object' && 'Message' in data) {
             const notification = data as { Message: string, Type?: string };
             const type = notification.Type || 'INFO';
             if (type === 'SUCCESS') success(notification.Message);
             else if (type === 'ERROR') error(notification.Message);
             else info(notification.Message);
        }
    };

    signalRService.on('ReceiveNotification', handleNotification);
    signalRService.on('ReceiveSystemNotification', handleNotification);
    signalRService.on('ReceiveOrderNotification', handleNotification);

    if (isAuthenticated && token) {
      signalRService.startConnection(() => token);
    }

    return () => {
        signalRService.off('ReceiveNotification', handleNotification);
        signalRService.off('ReceiveSystemNotification', handleNotification);
        signalRService.off('ReceiveOrderNotification', handleNotification);
    };
  }, [isAuthenticated, token, success, error, info]);

  if (isMaintenance) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center p-6 text-center">
        <div className="w-20 h-20 bg-amber-500/10 rounded-full flex items-center justify-center mb-6 animate-pulse">
          <ShieldAlert size={40} className="text-amber-500" />
        </div>
        <h1 className="text-2xl font-bold text-white mb-2 font-mono">VNVT OS RESTRICTED MODE</h1>
        <p className="text-slate-400 max-w-md text-sm mb-8">
          The core engine is currently undergoing index rebuilding and cache synchronization. 
          Please check back in 04:59:12.
        </p>
        <div className="space-y-2 w-full max-w-xs">
          <div className="h-1 bg-slate-800 rounded-full overflow-hidden">
            <motion.div 
              initial={{ width: 0 }}
              animate={{ width: '65%' }}
              className="h-full bg-amber-500"
            />
          </div>
          <div className="flex justify-between text-[10px] text-slate-500 font-mono">
            <span>SYNCING...</span>
            <span>65%</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <ServiceProvider services={services}>
        <QueryClientProvider client={queryClient}>
          <ConfirmProvider>
              <AppRouter />
              {/* <div>App Router Disabled for Build Test</div> */}
            <BuyNowModal />
            <ToastContainer />
          </ConfirmProvider>
        </QueryClientProvider>
    </ServiceProvider>
  );
}

export default App;
