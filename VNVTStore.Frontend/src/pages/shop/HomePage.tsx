import { useState, useEffect } from 'react';

import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import {
  ChevronRight,
  ArrowRight,
  Sparkles,
  Zap,
  Gift,
  TrendingUp,
  Loader2,
} from 'lucide-react';
import { ProductCard } from '@/components/common/ProductCard';
import { Button } from '@/components/ui';
import { useProducts, useCategories } from '@/hooks/useProducts';

import { HOME_BANNERS, BRAND_PARTNERS, FLASH_SALE_TIMES } from '@/data/homeData';
import { formatCurrency } from '@/utils/format';


// ============ Component ============



import { SectionHeader } from '@/components/common/SectionHeader';
import Image from '@/components/common/Image';

// ============ Component ============

// ============ Home Page Component ============
export const HomePage = () => {
  const { t } = useTranslation();
  const [currentSlide, setCurrentSlide] = useState(0);

  // Fetch products from API
  const { data: productsData, isLoading } = useProducts({
    pageIndex: 1,
    pageSize: 20,
  });

  // Fetch categories from API
  const { data: categories = [], isLoading: loadingCategories } = useCategories();


  const products = productsData?.products || [];

  const featuredProducts = products.slice(0, 8);
  const newProducts = products.slice(0, 4);
  const saleProducts = products.filter((p) => p.price > 0).slice(0, 4);

  // Auto slide
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % HOME_BANNERS.length);

    }, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="min-h-screen">
      {/* Hero Banner Slider */}
      <section className="relative h-[400px] md:h-[500px] lg:h-[600px] overflow-hidden">
        {HOME_BANNERS.map((banner, index) => (

          <motion.div
            key={banner.id}
            className={`absolute inset-0 ${index === currentSlide ? 'z-10' : 'z-0'}`}
            initial={{ opacity: 0 }}
            animate={{ opacity: index === currentSlide ? 1 : 0 }}
            transition={{ duration: 0.5 }}
          >
            {/* Background */}
            <div
              className="absolute inset-0 bg-cover bg-center"
              style={{ backgroundImage: `url(${banner.image})` }}
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
                  {t(`home.banners.${banner.id}.subtitle`)}
                </motion.span>

                <motion.h1
                  className="text-3xl md:text-4xl lg:text-5xl font-extrabold mb-4"
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.3 }}
                >
                  {t(`home.banners.${banner.id}.title`)}
                </motion.h1>

                <motion.p
                  className="text-lg md:text-xl opacity-90 mb-6"
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.4 }}
                >
                  {t(`home.banners.${banner.id}.description`)}
                </motion.p>

                <motion.div
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.5 }}
                >
                  <Link to={banner.link}>
                    <Button size="lg" rounded rightIcon={<ArrowRight size={20} />}>
                      {t(`home.banners.${banner.id}.cta`)}
                    </Button>
                  </Link>
                </motion.div>
              </div>
            </div>
          </motion.div>
        ))}

        {/* Dots */}
        <div className="absolute bottom-6 left-1/2 -translate-x-1/2 flex gap-2 z-20">
          {HOME_BANNERS.map((_, index) => (

            <button
              key={index}
              className={`w-3 h-3 rounded-full border-2 border-white transition-all ${index === currentSlide ? 'bg-white scale-110' : 'bg-transparent'
                }`}
              onClick={() => setCurrentSlide(index)}
            />
          ))}
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



          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
            {loadingCategories ? (
              Array.from({ length: 6 }).map((_, idx) => (
                <div key={idx} className="bg-primary rounded-xl p-4 flex flex-col items-center justify-center gap-3 animate-pulse">
                  <div className="w-24 h-24 bg-secondary/50 rounded-full" />
                  <div className="h-4 w-20 bg-secondary/50 rounded" />
                </div>
              ))
            ) : (
              categories.slice(0, 6).map((cat) => (
                <Link
                  key={cat.code}
                  to={`/products?category=${cat.code}`}
                  className="group bg-primary rounded-xl p-4 flex flex-col items-center justify-center gap-3 hover:shadow-lg transition-all border border-transparent hover:border-primary"
                >
                  <div className="w-24 h-24 rounded-full overflow-hidden bg-secondary/20 group-hover:scale-110 transition-transform">
                    <Image
                      src={cat.imageUrl}
                      alt={cat.name}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <div className="text-center">
                    <h3 className="font-semibold text-color-primary group-hover:text-primary transition-colors">
                      {cat.name}
                    </h3>
                  </div>
                </Link>
              )))}
          </div>

        </div>
      </section>

      {/* Flash Sale */}
      <section className="py-12 bg-gradient-to-r from-gray-900 to-gray-800 text-white">
        <div className="container mx-auto px-4">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-4">
              <Zap className="text-yellow-400 animate-pulse" size={28} />
              <h2 className="text-xl md:text-2xl font-bold">{t('home.flashSale')}</h2>
              <div className="flex gap-1">
                {FLASH_SALE_TIMES.map((num, i) => (
                  <span key={i} className="flex items-center">
                    <span className="px-2 py-1 bg-error rounded font-bold">{num}</span>
                    {i < 2 && <span className="mx-1">:</span>}
                  </span>
                ))}
              </div>
            </div>
            <Link
              to="/promotions"
              className="flex items-center gap-1 text-white/80 hover:text-white font-semibold transition-colors"
            >
              {t('common.viewAll')} <ChevronRight size={18} />
            </Link>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {saleProducts.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        </div>
      </section>

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
                <ProductCard key={product.id} product={product} />
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
                  <ProductCard key={product.id} product={product} />
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
                  key={product.id}
                  to={`/product/${product.id}`}
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
                  <Image
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

      {/* Brand Partners */}
      <section className="py-12 bg-secondary">
        <div className="container mx-auto px-4">
          <h2 className="text-xl font-bold text-center mb-8">{t('home.brands')}</h2>
          <div className="flex flex-wrap justify-center gap-4">
            {BRAND_PARTNERS.map(
              (brand) => (

                <div
                  key={brand}
                  className="px-6 py-3 bg-primary rounded-lg font-semibold text-secondary hover:bg-primary hover:text-white transition-colors cursor-pointer"
                >
                  {brand}
                </div>
              )
            )}
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;
