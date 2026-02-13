import { useEffect } from 'react';
import { useCartStore, useUIStore } from '@/store';
import { Outlet } from 'react-router-dom';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { QuickViewModal } from '@/components/common';
import { CartDrawer } from '@/components/common/CartDrawer';
import { ChatWidget } from '@/components/common/ChatWidget';
import { AnnouncementBanner } from '@/components/common/AnnouncementBanner';
import { FloatingContact } from '@/components/common/FloatingContact';

export const ShopLayout = () => {
  const { fetchCart } = useCartStore();
  const { quickViewProduct, setQuickViewProduct } = useUIStore();

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
      <FloatingContact />
      <QuickViewModal
        product={quickViewProduct}
        isOpen={!!quickViewProduct}
        onClose={() => setQuickViewProduct(null)}
      />
    </div>
  );
};

export default ShopLayout;
