import { memo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Facebook,
  Instagram,
  Youtube,
  Phone,
  Mail,
  MapPin,
  Send,
  CreditCard,
  Truck,
  Shield,
  HeadphonesIcon,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { useToast, useUIStore } from '@/store';

export const Footer = memo(() => {
  const { t } = useTranslation();
  const toast = useToast();
  const { theme, toggleTheme } = useUIStore();
  const [email, setEmail] = useState('');
  const [isSubscribing, setIsSubscribing] = useState(false);

  const features = [
    { icon: Truck, title: t('footer.features.shipping'), desc: t('footer.features.shippingDesc') },
    { icon: Shield, title: t('footer.features.warranty'), desc: t('footer.features.warrantyDesc') },
    { icon: CreditCard, title: t('footer.features.returns'), desc: t('footer.features.returnsDesc') },
    { icon: HeadphonesIcon, title: t('footer.features.support'), desc: t('footer.features.supportDesc') },
  ];

  const aboutLinks = [
    { label: t('header.about') || 'Giới thiệu', path: '/about' },
    { label: t('header.news') || 'Tin tức', path: '/news' },
    { label: t('header.promotions') || 'Khuyến mãi', path: '/promotions' },
    { label: t('header.contact') || 'Liên hệ', path: '/contact' },
  ];

  const supportLinks = [
    { label: t('header.trackOrder') || 'Tra cứu đơn hàng', path: '/tracking' },
    { label: t('header.support') || 'Hỗ trợ', path: '/support' },
    { label: t('common.products') || 'Sản phẩm', path: '/products' },
    { label: 'FAQ', path: '/support' },
  ];

  const handleSubscribe = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) {
      toast.error(t('messages.enterEmail') || 'Vui lòng nhập email');
      return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      toast.error(t('messages.invalidEmail') || 'Email không hợp lệ');
      return;
    }

    setIsSubscribing(true);
    // Simulate API call
    setTimeout(() => {
      toast.success(t('messages.subscribeSuccess') || 'Đăng ký nhận tin thành công!');
      setEmail('');
      setIsSubscribing(false);
    }, 1000);
  };

  return (
    <footer className="mt-auto">
      {/* Features */}
      <div className="bg-gradient-to-r from-indigo-600 to-purple-600 py-8">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((feature, index) => (
              <div key={index} className="flex items-center gap-4 text-white">
                <div className="w-14 h-14 bg-white/20 rounded-xl flex items-center justify-center backdrop-blur-sm">
                  <feature.icon size={28} />
                </div>
                <div>
                  <h4 className="font-semibold text-sm">{feature.title}</h4>
                  <p className="text-xs text-white/80">{feature.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Main Footer */}
      <div className="bg-gray-900 text-gray-300 py-12">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-8">
            {/* Company Info */}
            <div className="lg:col-span-2">
              <Link to="/" className="flex items-center gap-2 mb-4">
                <span className="text-2xl">🏠</span>
                <span className="flex flex-col leading-tight">
                  <span className="text-lg font-bold text-white">VNVT</span>
                  <span className="text-xs text-gray-500">Store</span>
                </span>
              </Link>
              <p className="text-sm text-gray-400 mb-6 max-w-sm">
                {t('footer.aboutDesc')}
              </p>

              <div className="space-y-2 mb-6">
                <div className="flex items-center gap-2 text-sm">
                  <MapPin size={16} className="text-indigo-400" />
                  <span>123 Nguyễn Huệ, Q.1, TP.HCM</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Phone size={16} className="text-indigo-400" />
                  <span>1900 123 456</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Mail size={16} className="text-indigo-400" />
                  <span>contact@vnvtstore.com</span>
                </div>
              </div>

              {/* Social */}
              <div className="flex gap-2">
                {[
                  { icon: Facebook, href: '#', color: 'hover:bg-blue-600' },
                  { icon: Instagram, href: '#', color: 'hover:bg-gradient-to-r hover:from-purple-500 hover:to-pink-500' },
                  { icon: Youtube, href: '#', color: 'hover:bg-red-600' },
                ].map((social, i) => (
                  <a
                    key={i}
                    href={social.href}
                    className={cn(
                      'w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center transition-all hover:text-white',
                      social.color
                    )}
                  >
                    <social.icon size={20} />
                  </a>
                ))}
                <a href="https://zalo.me" className="w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center hover:bg-blue-500 transition-colors">
                  <span className="text-xs font-bold">Zalo</span>
                </a>
              </div>
            </div>

            {/* Quick Links */}
            <div>
              <h3 className="text-white font-semibold mb-4 relative inline-block">
                {t('footer.about')}
                <span className="absolute -bottom-1 left-0 w-10 h-0.5 bg-gradient-to-r from-indigo-500 to-purple-500" />
              </h3>
              <ul className="space-y-2">
                {aboutLinks.map((link) => (
                  <li key={link.path}>
                    <Link to={link.path} className="text-sm text-gray-400 hover:text-white hover:pl-2 transition-all">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Support */}
            <div>
              <h3 className="text-white font-semibold mb-4 relative inline-block">
                {t('footer.support')}
                <span className="absolute -bottom-1 left-0 w-10 h-0.5 bg-gradient-to-r from-indigo-500 to-purple-500" />
              </h3>
              <ul className="space-y-2">
                {supportLinks.map((link, index) => (
                  <li key={index}>
                    <Link to={link.path} className="text-sm text-gray-400 hover:text-white hover:pl-2 transition-all">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Newsletter */}
            <div>
              <h3 className="text-white font-semibold mb-4 relative inline-block">
                {t('footer.newsletter')}
                <span className="absolute -bottom-1 left-0 w-10 h-0.5 bg-gradient-to-r from-indigo-500 to-purple-500" />
              </h3>
              <p className="text-sm text-gray-400 mb-4">
                {t('footer.newsletterDesc')}
              </p>
              <form onSubmit={handleSubscribe} className="flex gap-2">
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder={t('footer.emailPlaceholder')}
                  className="flex-1 px-4 py-2 bg-gray-800 border border-gray-700 rounded-lg text-sm focus:outline-none focus:border-indigo-500"
                />
                <button
                  type="submit"
                  disabled={isSubscribing}
                  className="w-11 h-11 bg-gradient-to-r from-indigo-600 to-purple-600 rounded-lg flex items-center justify-center hover:shadow-lg hover:shadow-indigo-500/25 transition-all disabled:opacity-50"
                >
                  <Send size={18} className="text-white" />
                </button>
              </form>

              {/* Payment Methods */}
              <div className="mt-6">
                <h4 className="text-xs text-gray-500 uppercase mb-2">{t('footer.paymentMethods')}</h4>
                <div className="flex flex-wrap gap-2">
                  {['Visa', 'MC', 'ZaloPay', 'MoMo', 'VNPAY'].map((method) => (
                    <div key={method} className="px-2 py-1 bg-white rounded text-xs font-bold text-gray-800">
                      {method}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Bottom */}
      <div className="bg-gray-950 py-4 border-t border-gray-800">
        <div className="container mx-auto px-4 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-sm text-gray-500">{t('footer.copyright')}</p>
          <div className="flex gap-4 items-center">
            <button
                onClick={toggleTheme}
                className="p-2 rounded-full hover:bg-white/10 transition-colors"
                title="Đổi giao diện"
            >
               <span className="text-xl">{theme === 'dark' ? '☀️' : '�'}</span>
            </button>
            <Link to="/about" className="text-sm text-gray-500 hover:text-white transition-colors">
              {t('header.about') || 'Về chúng tôi'}
            </Link>
            <Link to="/support" className="text-sm text-gray-500 hover:text-white transition-colors">
              {t('header.support') || 'Hỗ trợ'}
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
});

Footer.displayName = 'Footer';

export default Footer;
