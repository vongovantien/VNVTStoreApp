import { useState, useEffect, memo } from 'react';
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
import { useProducts } from '@/hooks';
import { mockCategories } from '@/data/mockData';
import { formatCurrency } from '@/utils/format';

// ============ Hero Banner Data ============
const banners = [
  {
    id: 1,
    title: 'Đồ gia dụng cao cấp',
    subtitle: 'Chất lượng tạo nên sự khác biệt',
    description: 'Giảm đến 50% cho đơn hàng đầu tiên',
    image: 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=1200',
    cta: 'Mua ngay',
    link: '/products',
  },
  {
    id: 2,
    title: 'Bộ sưu tập mới',
    subtitle: 'Xu hướng 2024',
    description: 'Thiết kế hiện đại - Chất liệu bền đẹp',
    image: 'https://images.unsplash.com/photo-1583845112203-29329902332e?w=1200',
    cta: 'Khám phá',
    link: '/products?new=true',
  },
  {
    id: 3,
    title: 'Flash Sale cuối tuần',
    subtitle: 'Chỉ còn 2 ngày',
    description: 'Giảm sốc đến 70% - Số lượng có hạn',
    image: 'https://images.unsplash.com/photo-1556909172-8c2f041fca1e?w=1200',
    cta: 'Xem ngay',
    link: '/promotions',
  },
];

// ============ Section Header Component ============
interface SectionHeaderProps {
  title: string;
  icon?: React.ReactNode;
  viewAllLink?: string;
  extra?: React.ReactNode;
}

const SectionHeader = memo(({ title, icon, viewAllLink, extra }: SectionHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="flex items-center justify-between mb-6">
      <h2 className="flex items-center gap-2 text-xl md:text-2xl font-bold text-primary">
        {icon}
        {title}
      </h2>
      <div className="flex items-center gap-4">
        {extra}
        {viewAllLink && (
          <Link
            to={viewAllLink}
            className="flex items-center gap-1 text-primary font-semibold hover:gap-2 transition-all"
          >
            {t('common.viewAll')} <ChevronRight size={18} />
          </Link>
        )}
      </div>
    </div>
  );
});

SectionHeader.displayName = 'SectionHeader';

// ============ Home Page Component ============
export const HomePage = () => {
  const { t } = useTranslation();
  const [currentSlide, setCurrentSlide] = useState(0);

  // Fetch products from API
  const { data: productsData, isLoading } = useProducts({
    pageIndex: 1,
    pageSize: 20,
  });

  const products = productsData?.products || [];
  const featuredProducts = products.slice(0, 8);
  const newProducts = products.slice(0, 4);
  const saleProducts = products.filter((p) => p.price > 0).slice(0, 4);

  // Auto slide
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % banners.length);
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="min-h-screen">
      {/* Hero Banner Slider */}
      <section className="relative h-[400px] md:h-[500px] lg:h-[600px] overflow-hidden">
        {banners.map((banner, index) => (
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
                  {banner.subtitle}
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
                  {banner.description}
                </motion.p>

                <motion.div
                  initial={{ y: 30, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.5 }}
                >
                  <Link to={banner.link}>
                    <Button size="lg" rounded rightIcon={<ArrowRight size={20} />}>
                      {banner.cta}
                    </Button>
                  </Link>
                </motion.div>
              </div>
            </div>
          </motion.div>
        ))}

        {/* Dots */}
        <div className="absolute bottom-6 left-1/2 -translate-x-1/2 flex gap-2 z-20">
          {banners.map((_, index) => (
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

          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {mockCategories.map((category, index) => (
              <motion.div
                key={category.id}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <Link
                  to={`/products?category=${category.slug}`}
                  className="flex flex-col items-center p-4 bg-primary rounded-xl hover:-translate-y-1 hover:shadow-xl transition-all group"
                >
                  <div className="w-16 h-16 md:w-20 md:h-20 rounded-lg overflow-hidden mb-3 bg-secondary">
                    {category.image && (
                      <img
                        src={category.image}
                        alt={category.name}
                        className="w-full h-full object-cover group-hover:scale-110 transition-transform"
                        loading="lazy"
                      />
                    )}
                  </div>
                  <h3 className="text-sm font-semibold text-center text-primary">
                    {category.name}
                  </h3>
                  <span className="text-xs text-tertiary">{category.productCount} SP</span>
                </Link>
              </motion.div>
            ))}
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
                {['02', '15', '30'].map((num, i) => (
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
              <span className="ml-2 text-secondary">Đang tải...</span>
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
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
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
                  Ưu đãi đặc biệt
                </span>
                <h3 className="text-2xl font-bold mb-2">Đăng ký thành viên</h3>
                <p className="opacity-90 mb-6">
                  Nhận ngay voucher 100K cho đơn hàng đầu tiên
                </p>
                <Link to="/register">
                  <Button className="bg-white text-primary hover:bg-gray-100">
                    Đăng ký ngay <ArrowRight size={18} />
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
                  <img
                    src={product.image}
                    alt={product.name}
                    className="w-14 h-14 object-cover rounded-lg"
                    loading="lazy"
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
            {['Apple', 'Samsung', 'Sony', 'LG', 'Panasonic', 'Philips', 'Xiaomi', 'Electrolux'].map(
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
