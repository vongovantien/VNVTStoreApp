import { useState, useEffect } from 'react';

import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import {
  ChevronRight,
  ChevronLeft,
  ArrowRight,
  Sparkles,
  Zap,
  Gift,
  TrendingUp,
  Loader2,
  Truck,
  ShieldCheck,
  RotateCcw,
  Headphones,
  Flame,
} from 'lucide-react';
import { ProductCard } from '@/components/common/ProductCard';
import SharedImage from '@/components/common/Image';
import { Button } from '@/components/ui';
import { useProducts, useCategories } from '@/hooks/useProducts';
import { useQuery } from '@tanstack/react-query';
import { promotionService, type Promotion } from '@/services/promotionService';
import { bannerService } from '@/services/bannerService';
import { systemConfigService } from '@/services/systemConfigService';
import { formatCurrency } from '@/utils/format';
import { useBrands } from '@/hooks/useBrands';


// ============ Component ============



import { SectionHeader } from '@/components/common/SectionHeader';
import { RecentlyViewed } from '@/components/common/RecentlyViewed';
import { useSEO, useOrganizationSchema } from '@/hooks/useSEO';

// ============ Component ============

// ============ Home Page Component ============
export const HomePage = () => {
  const { t } = useTranslation();
  const [currentSlide, setCurrentSlide] = useState(0);

  // SEO
  useSEO({
    title: 'Trang chủ',
    description: 'VNVT Store - Hệ thống cửa hàng đồ gia dụng cao cấp, chính hãng. Miễn phí vận chuyển toàn quốc, bảo hành 12-24 tháng.',
    canonicalPath: '/',
    keywords: 'đồ gia dụng, vnvt store, thiết bị nhà bếp, điện gia dụng, chính hãng, mua sắm trực tuyến',
  });
  useOrganizationSchema();

  // Fetch products from API
  const { data: productsData, isLoading } = useProducts({
    pageIndex: 1,
    pageSize: 20,
  });

  // Fetch categories from API
  const { data: categories = [], isLoading: loadingCategories } = useCategories();


  const { data: brandsData, isLoading: isLoadingBrands } = useBrands();
  const brandPartners = brandsData?.data?.items || [];

  const products = productsData?.products || [];

  const featuredProducts = products.slice(0, 8);
  const newProducts = products.slice(0, 4);
  // Fetch banners
  const { data: bannerData } = useQuery({
    queryKey: ['banners'],
    queryFn: () => bannerService.getAll(),
  });
  const banners = bannerData?.data?.items?.filter(b => b.isActive) || [];

  // Fetch flash sales
  const { data: flashSales } = useQuery({
    queryKey: ['flash-sales'],
    queryFn: () => promotionService.getFlashSales(),
  });

  const flashSaleProductCodes = flashSales?.data?.flatMap((p: Promotion) => p.productCodes || []) || [];
  
  // Fetch flash sale products
  const { data: flashSaleProductsData } = useProducts({
      pageIndex: 1,
      pageSize: 20,
      ids: flashSaleProductCodes,
      enabled: flashSaleProductCodes.length > 0
  });

  const saleProducts = flashSaleProductsData?.products || [];

  interface FlashSaleTimeSlot {
      active: boolean;
      label: string; // "09:00", "12:00" etc
  }

  // Countdown Timer Logic
  const [timeLeft, setTimeLeft] = useState<{hours: number, minutes: number, seconds: number}>({ hours: 0, minutes: 0, seconds: 0 });
  const [flashSaleConfig, setFlashSaleConfig] = useState<FlashSaleTimeSlot[]>([]);

  useEffect(() => {
    // Fetch Flash Sale Config
    const fetchConfig = async () => {
        try {
            const res = await systemConfigService.get('FLASHSALE_TIMES');
            if (res.success && res.data?.configValue) {
                setFlashSaleConfig(JSON.parse(res.data.configValue));
            }
        } catch {
            console.error("Failed to fetch flash sale config");
        }
    };
    fetchConfig();
  }, []);

  useEffect(() => {
    // Logic: Find the current active slot from config to determine END TIME
    // IF config is empty, fallback to existing logic (promotion endDate)
    
    let targetDate = 0;

    if (flashSaleConfig.length > 0) {
        // Simple logic: Find current active slot, End Time is the start of NEXT slot? 
        // Or assume slots are e.g. 09:00, 12:00. If now is 10:00, we are in 09:00 slot, end is 12:00.
        // If 20:00 is last slot, end is 24:00 (or determined by logic).
        
        const now = new Date();
        const currentHour = now.getHours();
        
        // Filter active slots and sort
        const activeSlots = flashSaleConfig
            .filter((s) => s.active)
            .map((s) => parseInt(s.label.split(':')[0]))
            .sort((a, b) => a - b);
            
        // Find slot we are currently in
        const currentSlotIndex = activeSlots.findIndex(h => currentHour >= h && (activeSlots.indexOf(h) === activeSlots.length - 1 || currentHour < activeSlots[activeSlots.indexOf(h) + 1]));
        
        if (currentSlotIndex !== -1) {
            const nextSlotHour = activeSlots[currentSlotIndex + 1];
            const endHour = nextSlotHour ? nextSlotHour : 24; // Default to midnight if last slot
            
            const target = new Date();
            target.setHours(endHour, 0, 0, 0);
            if (endHour === 24) { // Wrap to next day
                 target.setDate(target.getDate() + 1);
                 target.setHours(0, 0, 0, 0);
            }
            targetDate = target.getTime();
        }
    } 
    
    // Fallback if no config or no active slot found, use Promotion EndDate
    if (!targetDate && flashSales?.data) {
        const activeFlashSale = flashSales.data?.find((p: Promotion) => p.isActive && new Date(p.endDate) > new Date());
        if (activeFlashSale) {
            targetDate = new Date(activeFlashSale.endDate).getTime();
        }
    }

    if (!targetDate) return;

    const timer = setInterval(() => {
      const now = new Date().getTime();
      const distance = targetDate - now;

      if (distance < 0) {
        clearInterval(timer);
        setTimeLeft({ hours: 0, minutes: 0, seconds: 0 });
        return;
      }

      setTimeLeft({
        hours: Math.floor((distance / (1000 * 60 * 60))), // Total hours left
        minutes: Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60)),
        seconds: Math.floor((distance % (1000 * 60)) / 1000)
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [flashSales?.data, flashSaleConfig]);

  // Auto slide
  useEffect(() => {
    const interval = setInterval(() => {
      if (banners.length > 0) {
          setCurrentSlide((prev) => (prev + 1) % banners.length);
      }
    }, 5000);
    return () => clearInterval(interval);
  }, [banners.length]);

  return (
    <div className="min-h-screen">
      {/* Hero Banner Slider */}
      <section className="relative h-[400px] md:h-[500px] lg:h-[600px] overflow-hidden">
        {banners.map((banner, index) => (

          <motion.div
            key={banner.code}
            className={`absolute inset-0 ${index === currentSlide ? 'z-10' : 'z-0'}`}
            initial={{ opacity: 0 }}
            animate={{ opacity: index === currentSlide ? 1 : 0 }}
            transition={{ duration: 0.5 }}
          >
            {/* Background */}
            <div
              className="absolute inset-0 bg-cover bg-center"
              style={{ backgroundImage: `url(${banner.imageURL})` }}
            />
            <div className="absolute inset-0 bg-gradient-to-r from-black/70 via-black/40 to-transparent" />

            {/* Content */}
            <div className="container mx-auto px-4 h-full flex items-center relative z-10">
              <div className="max-w-xl text-white">
                <motion.span
                  className="inline-flex items-center gap-2 px-4 py-2 bg-white/20 backdrop-blur-sm rounded-full text-sm font-semibold mb-4"
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.2 }}
                >
                  <Sparkles size={16} />
                  {banner.content || t('home.banner.specialOffer')}
                </motion.span>

                <motion.h1
                  className="text-3xl md:text-4xl lg:text-5xl font-extrabold mb-4"
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.3 }}
                >
                  {banner.title}
                </motion.h1>

                <motion.p
                  className="text-lg md:text-xl opacity-90 mb-6"
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.4 }}
                >
                  {banner.content}
                </motion.p>

                <motion.div
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.5 }}
                >
                  <Link to={banner.linkUrl || '#'}>
                    <Button size="lg" rounded rightIcon={<ArrowRight size={20} />}>
                      {banner.linkText || t('common.viewNow')}
                    </Button>
                  </Link>
                </motion.div>
              </div>
            </div>
          </motion.div>
        ))}

        {/* Navigation Arrows */}
        {banners.length > 1 && (
          <>
            <button
              onClick={() => setCurrentSlide((prev) => (prev === 0 ? banners.length - 1 : prev - 1))}
              className="absolute left-4 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full glass-panel flex items-center justify-center text-white/90 hover:text-white hover:scale-110 transition-all z-20 shadow-lg cursor-pointer"
              aria-label="Previous Slide"
            >
              <ChevronLeft size={24} />
            </button>
            <button
              onClick={() => setCurrentSlide((prev) => (prev === banners.length - 1 ? 0 : prev + 1))}
              className="absolute right-4 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full glass-panel flex items-center justify-center text-white/90 hover:text-white hover:scale-110 transition-all z-20 shadow-lg cursor-pointer"
              aria-label="Next Slide"
            >
              <ChevronRight size={24} />
            </button>
          </>
        )}

        {/* Dots */}
        <div className="absolute bottom-6 left-1/2 -translate-x-1/2 flex gap-2 z-20">
          {banners.map((_, index) => (
            <button
              key={index}
              className={`h-2.5 rounded-full transition-all duration-300 ${index === currentSlide ? 'w-8 bg-white shadow-md' : 'w-2.5 bg-white/40 hover:bg-white/70'}`}
              onClick={() => setCurrentSlide(index)}
              aria-label={`Slide ${index + 1}`}
            />
          ))}
        </div>
      </section>

      {/* Value Proposition Bar */}
      <section className="relative z-20 -mt-6 max-w-7xl mx-auto px-4">
        <div className="glass-panel rounded-2xl shadow-xl p-4 md:p-6 grid grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6 border border-slate-200/80 dark:border-slate-800/80">
          <div className="flex items-center gap-3.5 p-2 rounded-xl hover:bg-black/5 dark:hover:bg-white/5 transition-colors">
            <div className="w-12 h-12 rounded-xl gradient-primary flex items-center justify-center text-white shadow-md flex-shrink-0">
              <Truck size={22} />
            </div>
            <div>
              <h4 className="font-bold text-sm md:text-base text-text-primary">Giao Hàng Siêu Tốc</h4>
              <p className="text-xs text-text-tertiary">Miễn phí cho đơn từ 500k</p>
            </div>
          </div>
          <div className="flex items-center gap-3.5 p-2 rounded-xl hover:bg-black/5 dark:hover:bg-white/5 transition-colors">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white shadow-md flex-shrink-0">
              <ShieldCheck size={22} />
            </div>
            <div>
              <h4 className="font-bold text-sm md:text-base text-text-primary">100% Chính Hãng</h4>
              <p className="text-xs text-text-tertiary">Cam kết hoàn tiền gấp 2</p>
            </div>
          </div>
          <div className="flex items-center gap-3.5 p-2 rounded-xl hover:bg-black/5 dark:hover:bg-white/5 transition-colors">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white shadow-md flex-shrink-0">
              <RotateCcw size={22} />
            </div>
            <div>
              <h4 className="font-bold text-sm md:text-base text-text-primary">Đổi Trả 7 Ngày</h4>
              <p className="text-xs text-text-tertiary">Thủ tục nhanh chóng, an tâm</p>
            </div>
          </div>
          <div className="flex items-center gap-3.5 p-2 rounded-xl hover:bg-black/5 dark:hover:bg-white/5 transition-colors">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-rose-500 to-pink-600 flex items-center justify-center text-white shadow-md flex-shrink-0">
              <Headphones size={22} />
            </div>
            <div>
              <h4 className="font-bold text-sm md:text-base text-text-primary">Hỗ Trợ 24/7</h4>
              <p className="text-xs text-text-tertiary">Đội ngũ kỹ thuật tận tâm</p>
            </div>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="py-12 bg-secondary">
        <div className="container mx-auto px-4">
          <SectionHeader
            title={t('home.categories')}
            icon={<span className="text-2xl">🏷️</span>}
            viewAllLink="/products"
          />

          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
            {loadingCategories ? (
              Array.from({ length: 6 }).map((_, idx) => (
                <div key={idx} className="glass-card rounded-2xl p-5 flex flex-col items-center justify-center gap-3 animate-pulse">
                  <div className="w-20 h-20 bg-secondary/60 rounded-full" />
                  <div className="h-4 w-20 bg-secondary/60 rounded" />
                </div>
              ))
            ) : (
              categories.slice(0, 6).map((cat) => (
                <Link
                  key={cat.code}
                  to={`/products?category=${cat.code}`}
                  className="group glass-card rounded-2xl p-5 flex flex-col items-center justify-center gap-3 hover-lift border border-slate-200/60 dark:border-slate-800/60 hover:border-indigo-500/50 dark:hover:border-indigo-400/50 transition-all duration-300"
                >
                  <div className="w-20 h-20 rounded-full overflow-hidden bg-secondary/40 p-1 group-hover:scale-110 transition-transform duration-300 shadow-inner">
                    <SharedImage
                      src={cat.imageURL}
                      alt={cat.name}
                      className="w-full h-full object-cover rounded-full"
                    />
                  </div>
                  <div className="text-center">
                    <h3 className="font-semibold text-sm md:text-base text-text-primary group-hover:text-accent-primary transition-colors">
                      {cat.name}
                    </h3>
                  </div>
                </Link>
              )))}
          </div>
        </div>
      </section>

      {/* Flash Sale */}
      {/* Flash Sale - Only show if we have active sales */}
      {saleProducts.length > 0 && (
      <section className="py-12 relative overflow-hidden bg-gradient-to-br from-slate-950 via-indigo-950 to-slate-900 text-white rounded-3xl mx-4 my-8 shadow-2xl border border-indigo-500/20">
        <div className="absolute top-0 right-0 w-96 h-96 bg-rose-500/10 rounded-full blur-3xl pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-indigo-500/10 rounded-full blur-3xl pointer-events-none" />
        <div className="container mx-auto px-6 relative z-10">
          <div className="flex flex-col md:flex-row items-center justify-between mb-8 gap-4 border-b border-white/10 pb-6">
            <div className="flex flex-wrap items-center gap-4">
              <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-gradient-to-r from-amber-500 to-rose-600 font-bold shadow-lg">
                <Flame className="text-yellow-300 animate-bounce" size={24} />
                <span className="tracking-wide uppercase text-sm md:text-base">{t('shared.flashSale')}</span>
              </div>
              <span className="text-white/60 text-sm hidden sm:inline">Kết thúc trong:</span>
              {/* Dynamic Countdown */}
              <div className="flex items-center gap-2 font-mono text-base md:text-lg font-black">
                 <div className="bg-slate-900/90 border border-rose-500/40 px-3 py-1.5 rounded-lg shadow-inner text-rose-400">{String(timeLeft.hours).padStart(2, '0')}</div>
                 <span className="text-rose-500 font-bold animate-pulse">:</span>
                 <div className="bg-slate-900/90 border border-rose-500/40 px-3 py-1.5 rounded-lg shadow-inner text-rose-400">{String(timeLeft.minutes).padStart(2, '0')}</div>
                 <span className="text-rose-500 font-bold animate-pulse">:</span>
                 <div className="bg-slate-900/90 border border-rose-500/40 px-3 py-1.5 rounded-lg shadow-inner text-rose-400">{String(timeLeft.seconds).padStart(2, '0')}</div>
              </div>
            </div>
            <Link
              to="/promotions"
              className="flex items-center gap-1.5 px-4 py-2 rounded-full glass-panel text-white/90 hover:text-white font-semibold transition-all hover:scale-105 border border-white/20 text-sm"
            >
              {t('common.viewAll')} <ArrowRight size={16} />
            </Link>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {saleProducts.map((product) => (
              <ProductCard key={product.code} product={product} />
            ))}
          </div>
        </div>
      </section>
      )}

      {/* Featured Products */}
      <section className="py-12">
        <div className="container mx-auto px-4">
          <SectionHeader
            title={t('home.featured')}
            icon={<span className="text-2xl">⭐</span>}
            viewAllLink="/products?featured=true"
          />

          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="w-6 h-6 animate-spin text-accent" />
              <span className="ml-2 text-secondary">{t('common.loading')}</span>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {featuredProducts.map((product) => (
                <ProductCard key={product.code} product={product} />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* New Arrivals + Promo Banner */}
      <section className="py-12 bg-secondary">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* New Arrivals */}
            <div className="lg:col-span-2">
              <SectionHeader
                title={t('home.newArrivals')}
                icon={<span className="text-2xl">🆕</span>}
                viewAllLink="/products?new=true"
              />
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {newProducts.map((product) => (
                  <ProductCard key={product.code} product={product} />
                ))}
              </div>
            </div>

            {/* Promo Banner */}
            <div className="relative bg-gradient-to-br from-primary to-purple-500 rounded-2xl overflow-hidden min-h-[400px] flex flex-col justify-end">
              <img
                src="https://images.unsplash.com/photo-1556909114-44e3e70034e2?w=400"
                alt="Promo"
                className="absolute inset-0 w-full h-full object-cover opacity-30"
              />
              <div className="relative z-10 p-6 text-white">
                <span className="inline-flex items-center gap-2 px-3 py-1 bg-white/20 rounded-full text-sm font-semibold mb-4">
                  <Gift size={16} />
                  {t('home.specialOffer')}
                </span>
                <h3 className="text-2xl font-bold mb-2">{t('home.registerMember')}</h3>
                <p className="opacity-90 mb-6">
                  {t('home.voucherOffer')}
                </p>
                <Link to="/register">
                  <Button className="bg-white text-primary hover:bg-gray-100">
                    {t('home.registerNow')} <ArrowRight size={18} />
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Best Sellers */}
      <section className="py-12">
        <div className="container mx-auto px-4">
          <SectionHeader
            title={t('home.bestSellers')}
            icon={<TrendingUp size={24} />}
            viewAllLink="/products?sort=bestseller"
          />

          <div className="space-y-4">
            {isLoading ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="w-6 h-6 animate-spin text-accent" />
              </div>
            ) : (
              products.slice(0, 5).map((product, index) => (
                <Link
                  key={product.code}
                  to={`/product/${product.code}`}
                  className="flex items-center gap-4 p-4 bg-primary rounded-xl hover:shadow-lg hover:translate-x-1 transition-all"
                >
                  <span
                    className={`w-9 h-9 flex items-center justify-center font-bold rounded-lg text-white ${index === 0
                      ? 'bg-gradient-to-r from-yellow-500 to-yellow-300'
                      : index === 1
                        ? 'bg-gradient-to-r from-gray-400 to-gray-300'
                        : index === 2
                          ? 'bg-gradient-to-r from-amber-700 to-amber-500'
                          : 'bg-primary'
                      }`}
                  >
                    {index + 1}
                  </span>
                  <SharedImage
                    src={product.image}
                    alt={product.name}
                    className="w-14 h-14 object-cover rounded-lg"
                  />
                  <div className="flex-1">
                    <h4 className="font-semibold text-primary line-clamp-1">{product.name}</h4>
                    <span className="text-sm font-bold text-error">
                      {formatCurrency(product.price)}
                    </span>
                  </div>
                </Link>
              ))
            )}
          </div>
        </div>
      </section>

      {/* Recently Viewed */}
      <RecentlyViewed />

      {/* Brand Partners */}
      <section className="py-12 bg-secondary">
        <div className="container mx-auto px-4">
          <h2 className="text-xl font-bold text-center mb-8">{t('home.brands')}</h2>
          <div className="flex flex-wrap justify-center gap-4">
            {isLoadingBrands ? (
                Array.from({ length: 4 }).map((_, idx) => (
                    <div key={idx} className="w-32 h-12 bg-primary animate-pulse rounded-lg" />
                ))
            ) : (
                brandPartners.map((brand) => (
                    <div
                      key={brand.code}
                      className="px-6 py-3 bg-primary rounded-lg font-semibold text-secondary hover:bg-accent hover:text-white transition-colors cursor-pointer"
                    >
                      {brand.name}
                    </div>
                ))
            )}
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;
