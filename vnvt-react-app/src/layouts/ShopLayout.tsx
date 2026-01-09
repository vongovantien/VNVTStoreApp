import { useEffect } from 'react';
import { useCartStore } from '@/store';
import { Outlet } from 'react-router-dom';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { CartDrawer } from '@/components/common/CartDrawer';
import { ChatWidget } from '@/components/common/ChatWidget';
import { AnnouncementBanner } from '@/components/common/AnnouncementBanner';

export const ShopLayout = () => {
  const { fetchCart } = useCartStore();

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  return (
    <div className="flex flex-col min-h-screen">
      <AnnouncementBanner />
      <Header />
      <main className="flex-1">
        <Outlet />
      </main>
      <Footer />
      <CartDrawer />
      <ChatWidget />
    </div>
  );
};

export default ShopLayout;
