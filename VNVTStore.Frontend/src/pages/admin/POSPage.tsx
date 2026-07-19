import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Search, Barcode, ShoppingCart, CreditCard, Banknote,
  Plus, Minus, Trash2, Tag, ChevronRight, CheckCircle2,
  Loader2, ReceiptText, RefreshCw, X, Keyboard, Clock
} from 'lucide-react';
import { useProducts } from '@/hooks';
import { formatCurrency } from '@/utils/format';
import { orderService } from '@/services/orderService';
import { useToast } from '@/store';
import defaultImage from '@/assets/default-image.png';
import type { Product } from '@/types';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';

// ─── Types ─────────────────────────────────────────────────────────────────────
interface CartItem {
  product: Product;
  quantity: number;
}

type PaymentMethod = 'cash' | 'card' | 'transfer';

// ─── Payment Method Config ──────────────────────────────────────────────────────
const PAYMENT_METHODS: { id: PaymentMethod; label: string; icon: React.ElementType; shortcut: string }[] = [
  { id: 'cash',     label: 'Tiền mặt',     icon: Banknote,    shortcut: 'F9' },
  { id: 'card',     label: 'Thẻ',          icon: CreditCard,  shortcut: 'F8' },
  { id: 'transfer', label: 'Chuyển khoản', icon: ReceiptText, shortcut: 'F7' },
];

const CATEGORIES_QUICK = ['Tất cả', 'Bán chạy', 'Nhà bếp', 'Điện tử', 'Gia dụng'];

// ─── Sub-components ─────────────────────────────────────────────────────────────

const ProductCard = ({ product, onAdd }: { product: Product; onAdd: (p: Product) => void }) => {
  const isOutOfStock = (product.stock ?? product.stockQuantity ?? 0) === 0;
  return (
    <button
      onClick={() => !isOutOfStock && onAdd(product)}
      disabled={isOutOfStock}
      className={cn(
        'group relative flex flex-col justify-between text-left p-3 rounded-xl border transition-all duration-200',
        'bg-white dark:bg-slate-900 hover:border-blue-500 dark:hover:border-blue-400',
        'border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md',
        isOutOfStock && 'opacity-50 cursor-not-allowed hover:border-slate-200 dark:hover:border-slate-800 hover:shadow-sm'
      )}
    >
      {/* Image */}
      <div className="aspect-square rounded-lg overflow-hidden bg-slate-100 dark:bg-slate-800 mb-2">
        <img
          src={product.image || defaultImage}
          alt={product.name}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          onError={(e) => { (e.target as HTMLImageElement).src = defaultImage; }}
        />
      </div>
      {/* Info */}
      <div>
        <p className="text-[10px] font-mono text-slate-400 truncate">{product.code}</p>
        <h3 className="font-semibold text-xs leading-tight line-clamp-2 mt-0.5 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
          {product.name}
        </h3>
        <div className="flex justify-between items-end mt-2 gap-1">
          <span className={cn(
            'text-[10px] font-medium px-1.5 py-0.5 rounded-full',
            isOutOfStock
              ? 'bg-red-50 dark:bg-red-950/30 text-red-500'
              : 'bg-emerald-50 dark:bg-emerald-950/30 text-emerald-600 dark:text-emerald-400'
          )}>
            {isOutOfStock ? 'Hết hàng' : `Tồn: ${product.stock ?? product.stockQuantity}`}
          </span>
          <span className="text-sm font-black text-blue-600 dark:text-blue-400 shrink-0">
            {formatCurrency(product.price)}
          </span>
        </div>
      </div>
      {/* Add overlay */}
      {!isOutOfStock && (
        <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity rounded-xl bg-blue-600/10">
          <div className="bg-blue-600 text-white rounded-full p-2 shadow-lg">
            <Plus size={18} />
          </div>
        </div>
      )}
    </button>
  );
};

const CartRow = ({
  item, onIncrease, onDecrease, onRemove
}: {
  item: CartItem;
  onIncrease: () => void;
  onDecrease: () => void;
  onRemove: () => void;
}) => (
  <div className="flex items-start gap-3 py-3 border-b border-slate-100 dark:border-slate-800 last:border-0">
    <img
      src={item.product.image || defaultImage}
      alt={item.product.name}
      className="w-10 h-10 rounded-lg object-cover border border-slate-200 dark:border-slate-700 shrink-0"
      onError={(e) => { (e.target as HTMLImageElement).src = defaultImage; }}
    />
    <div className="flex-1 min-w-0">
      <p className="text-xs font-semibold leading-tight line-clamp-2">{item.product.name}</p>
      <p className="text-[10px] text-slate-400 mt-0.5">{formatCurrency(item.product.price)} / sp</p>
    </div>
    <div className="flex flex-col items-end gap-1.5 shrink-0">
      <p className="text-sm font-black text-blue-600 dark:text-blue-400">
        {formatCurrency(item.product.price * item.quantity)}
      </p>
      <div className="flex items-center gap-1">
        <button
          onClick={onDecrease}
          className="w-6 h-6 rounded border border-slate-200 dark:border-slate-700 flex items-center justify-center hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-500 transition-colors"
        >
          <Minus size={12} />
        </button>
        <span className="text-xs font-bold w-6 text-center">{item.quantity}</span>
        <button
          onClick={onIncrease}
          className="w-6 h-6 rounded border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-950/30 flex items-center justify-center hover:bg-blue-100 dark:hover:bg-blue-900/50 text-blue-600 transition-colors"
        >
          <Plus size={12} />
        </button>
        <button
          onClick={onRemove}
          className="w-6 h-6 rounded border border-red-100 dark:border-red-900 bg-red-50 dark:bg-red-950/30 flex items-center justify-center hover:bg-red-100 dark:hover:bg-red-900/50 text-red-500 transition-colors ml-1"
        >
          <Trash2 size={12} />
        </button>
      </div>
    </div>
  </div>
);

// ─── Main POS Page ──────────────────────────────────────────────────────────────

export const POSPage = () => {
  const { t } = useTranslation();
  const toast = useToast();

  // State
  const [search, setSearch] = useState('');
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [cart, setCart] = useState<CartItem[]>([]);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('cash');
  const [isProcessing, setIsProcessing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [discountAmount, setDiscountAmount] = useState(0);
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [cashReceived, setCashReceived] = useState('');
  const [showShortcuts, setShowShortcuts] = useState(false);
  const [lastOrderTime, setLastOrderTime] = useState<Date | null>(null);
  const [now, setNow] = useState(new Date());

  // Live clock — ticks every second
  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  const formattedDate = now.toLocaleDateString('vi-VN', {
    weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric'
  });
  const formattedTime = now.toLocaleTimeString('vi-VN', {
    hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
  });

  const searchRef = useRef<HTMLInputElement>(null);

  // Fetch products
  const { data: productsData, isLoading, refetch } = useProducts({
    pageSize: 24,
    ...(search ? { search } : {}),
  });
  const products: Product[] = productsData?.products || [];

  // ── Cart helpers ──────────────────────────────────────────────────────────────
  const addToCart = useCallback((product: Product) => {
    setCart(prev => {
      const existing = prev.find(i => i.product.code === product.code);
      if (existing) {
        return prev.map(i =>
          i.product.code === product.code ? { ...i, quantity: i.quantity + 1 } : i
        );
      }
      return [...prev, { product, quantity: 1 }];
    });
  }, []);

  const removeFromCart = useCallback((code: string) => {
    setCart(prev => prev.filter(i => i.product.code !== code));
  }, []);

  const changeQty = useCallback((code: string, delta: number) => {
    setCart(prev =>
      prev
        .map(i => i.product.code === code ? { ...i, quantity: i.quantity + delta } : i)
        .filter(i => i.quantity > 0)
    );
  }, []);

  const clearCart = useCallback(() => {
    setCart([]);
    setDiscountAmount(0);
    setCustomerName('');
    setCustomerPhone('');
    setCashReceived('');
  }, []);

  // ── Calculations ───────────────────────────────────────────────────────────────
  const subtotal = cart.reduce((sum, i) => sum + i.product.price * i.quantity, 0);
  const total = Math.max(0, subtotal - discountAmount);
  const cashChange = paymentMethod === 'cash' && cashReceived
    ? Number(cashReceived.replace(/,/g, '')) - total
    : 0;

  // ── Process order ──────────────────────────────────────────────────────────────
  const handleProcessPayment = async () => {
    if (cart.length === 0) {
      toast.error('Giỏ hàng trống, vui lòng thêm sản phẩm');
      return;
    }
    setIsProcessing(true);
    try {
      await orderService.create({
        fullName: customerName || 'Khách lẻ',
        phone: customerPhone || '0000000000',
        address: 'Tại quầy',
        city: 'HCM',
        district: 'POS',
        ward: 'POS',
        paymentMethod,
        items: cart.map(i => ({ productCode: i.product.code, quantity: i.quantity })),
      });
      setShowSuccess(true);
      setLastOrderTime(new Date());
      clearCart();
      setTimeout(() => setShowSuccess(false), 5000);
      toast.success('Đơn hàng đã được tạo thành công!');
    } catch (err: any) {
      toast.error(err.message || 'Tạo đơn thất bại');
    } finally {
      setIsProcessing(false);
    }
  };

  // ── Keyboard shortcuts ─────────────────────────────────────────────────────────
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'F2') { e.preventDefault(); searchRef.current?.focus(); }
      if (e.key === 'F7') { e.preventDefault(); setPaymentMethod('transfer'); }
      if (e.key === 'F8') { e.preventDefault(); setPaymentMethod('card'); }
      if (e.key === 'F9') { e.preventDefault(); setPaymentMethod('cash'); }
      if (e.key === 'F10') { e.preventDefault(); handleProcessPayment(); }
      if (e.key === 'Escape') { searchRef.current?.blur(); }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [cart, paymentMethod]);

  // ─── Render ───────────────────────────────────────────────────────────────────
  return (
    <div className="flex flex-col h-[calc(100vh-120px)] overflow-hidden">
      {/* ── Top Bar ── */}
      <div className="flex items-center gap-3 mb-4 flex-wrap">
        {/* Search */}
        <div className="relative flex-1 min-w-[200px] max-w-md">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            ref={searchRef}
            type="text"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Tìm sản phẩm, SKU... (F2)"
            className="w-full pl-9 pr-4 py-2.5 text-sm border border-slate-200 dark:border-slate-700 rounded-xl bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
          />
        </div>
        {/* Live clock */}
        <div className="flex items-center gap-2 px-3 py-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-xl text-xs font-semibold text-slate-700 dark:text-slate-300 tabular-nums">
          <Clock size={14} className="text-blue-500 shrink-0" />
          <div className="flex flex-col leading-tight">
            <span className="text-slate-400 dark:text-slate-500 text-[10px] font-medium">{formattedDate}</span>
            <span className="text-blue-600 dark:text-blue-400 font-black tracking-wide">{formattedTime}</span>
          </div>
        </div>
        {/* Barcode indicator */}
        <div className="flex items-center gap-2 px-3 py-2.5 bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 rounded-xl text-xs font-semibold text-blue-600 dark:text-blue-400">
          <Barcode size={15} />
          <span>Barcode: Sẵn sàng</span>
        </div>
        {/* Refresh */}
        <button
          onClick={() => refetch()}
          className="p-2.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-500 transition-colors"
          title="Làm mới"
        >
          <RefreshCw size={16} />
        </button>
        {/* Shortcuts toggle */}
        <button
          onClick={() => setShowShortcuts(v => !v)}
          className="p-2.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-500 transition-colors"
          title="Phím tắt"
        >
          <Keyboard size={16} />
        </button>
      </div>

      {/* Shortcuts reference */}
      {showShortcuts && (
        <div className="mb-4 p-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-xl grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs">
          {[
            ['F2', 'Tìm kiếm'],
            ['F7', 'Chuyển khoản'],
            ['F8', 'Thẻ'],
            ['F9', 'Tiền mặt'],
            ['F10', 'Thanh toán'],
            ['Esc', 'Bỏ chọn'],
          ].map(([key, desc]) => (
            <div key={key} className="flex items-center gap-2">
              <kbd className="px-2 py-0.5 rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 font-mono font-bold text-slate-600 dark:text-slate-300 shadow-sm">
                {key}
              </kbd>
              <span className="text-slate-500">{desc}</span>
            </div>
          ))}
        </div>
      )}

      {/* ── Main Content ── */}
      <div className="flex flex-1 gap-4 overflow-hidden">

        {/* ── Left: Product Grid ── */}
        <section className="flex-1 flex flex-col overflow-hidden min-w-0">
          {/* Category tabs */}
          <div className="flex gap-2 overflow-x-auto pb-3 no-scrollbar shrink-0">
            {CATEGORIES_QUICK.map(cat => (
              <button
                key={cat}
                onClick={() => setActiveCategory(cat)}
                className={cn(
                  'px-4 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap transition-all border',
                  activeCategory === cat
                    ? 'bg-blue-600 text-white border-blue-600 shadow-md shadow-blue-500/20'
                    : 'bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 text-slate-600 dark:text-slate-400 hover:border-blue-400 hover:text-blue-600'
                )}
              >
                {cat}
              </button>
            ))}
          </div>

          {/* Product Grid */}
          <div className="flex-1 overflow-y-auto pr-1">
            {isLoading ? (
              <div className="flex items-center justify-center h-48 text-slate-400">
                <Loader2 size={32} className="animate-spin" />
              </div>
            ) : products.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-48 text-slate-400 gap-2">
                <Search size={32} />
                <p className="text-sm">Không tìm thấy sản phẩm</p>
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-3 pb-2">
                {products.map(product => (
                  <ProductCard key={product.code} product={product} onAdd={addToCart} />
                ))}
              </div>
            )}
          </div>
        </section>

        {/* ── Right: Cart & Checkout Pane ── */}
        <aside className="w-80 xl:w-96 shrink-0 flex flex-col bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-sm overflow-hidden">

          {/* Cart Header */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-slate-100 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-900/60 shrink-0">
            <span className="font-bold text-sm flex items-center gap-2">
              <ShoppingCart size={16} className="text-blue-600" />
              Đơn hàng ({cart.reduce((s, i) => s + i.quantity, 0)} sp)
            </span>
            {cart.length > 0 && (
              <button
                onClick={clearCart}
                className="text-xs text-red-500 hover:text-red-700 hover:underline transition-colors flex items-center gap-1"
              >
                <X size={12} /> Xóa tất cả
              </button>
            )}
          </div>

          {/* Customer info */}
          <div className="px-3 py-2.5 border-b border-slate-100 dark:border-slate-800 grid grid-cols-2 gap-2 shrink-0">
            <input
              type="text"
              value={customerName}
              onChange={e => setCustomerName(e.target.value)}
              placeholder="Tên khách hàng"
              className="w-full px-2.5 py-1.5 text-xs border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            <input
              type="text"
              value={customerPhone}
              onChange={e => setCustomerPhone(e.target.value)}
              placeholder="Số điện thoại"
              className="w-full px-2.5 py-1.5 text-xs border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          {/* Cart items */}
          <div className="flex-1 overflow-y-auto px-3 py-1">
            {cart.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-slate-400 gap-3 py-12">
                <ShoppingCart size={36} strokeWidth={1} />
                <p className="text-xs text-center">Chưa có sản phẩm nào<br/>Click vào sản phẩm để thêm</p>
              </div>
            ) : (
              cart.map(item => (
                <CartRow
                  key={item.product.code}
                  item={item}
                  onIncrease={() => changeQty(item.product.code, 1)}
                  onDecrease={() => changeQty(item.product.code, -1)}
                  onRemove={() => removeFromCart(item.product.code)}
                />
              ))
            )}
          </div>

          {/* Discount */}
          <div className="px-3 py-2 border-t border-slate-100 dark:border-slate-800 shrink-0">
            <div className="flex items-center gap-2">
              <Tag size={14} className="text-slate-400 shrink-0" />
              <input
                type="number"
                value={discountAmount || ''}
                onChange={e => setDiscountAmount(Number(e.target.value))}
                placeholder="Giảm giá (đ)"
                min={0}
                max={subtotal}
                className="flex-1 px-2.5 py-1.5 text-xs border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Summary */}
          <div className="px-4 py-3 border-t border-slate-100 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-900/60 space-y-1.5 shrink-0">
            <div className="flex justify-between text-xs text-slate-500">
              <span>Tạm tính</span>
              <span>{formatCurrency(subtotal)}</span>
            </div>
            {discountAmount > 0 && (
              <div className="flex justify-between text-xs text-red-500">
                <span>Giảm giá</span>
                <span>-{formatCurrency(discountAmount)}</span>
              </div>
            )}
            <div className="flex justify-between font-black text-base pt-1.5 border-t border-slate-200 dark:border-slate-700">
              <span>Tổng cộng</span>
              <span className="text-blue-600 dark:text-blue-400">{formatCurrency(total)}</span>
            </div>
          </div>

          {/* Payment methods */}
          <div className="px-3 py-2 border-t border-slate-100 dark:border-slate-800 shrink-0">
            <p className="text-[10px] text-slate-400 font-semibold uppercase tracking-wider mb-2">Phương thức</p>
            <div className="grid grid-cols-3 gap-1.5">
              {PAYMENT_METHODS.map(method => {
                const Icon = method.icon;
                return (
                  <button
                    key={method.id}
                    onClick={() => setPaymentMethod(method.id)}
                    className={cn(
                      'flex flex-col items-center gap-1 py-2 px-1 rounded-lg border text-xs font-semibold transition-all',
                      paymentMethod === method.id
                        ? 'bg-blue-600 text-white border-blue-600 shadow-md shadow-blue-500/20'
                        : 'bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 text-slate-600 dark:text-slate-400 hover:border-blue-400'
                    )}
                  >
                    <Icon size={14} />
                    <span>{method.label}</span>
                    <span className={cn(
                      'text-[8px] font-mono',
                      paymentMethod === method.id ? 'text-blue-100' : 'text-slate-400'
                    )}>{method.shortcut}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Cash change calculator */}
          {paymentMethod === 'cash' && (
            <div className="px-3 py-2 border-t border-slate-100 dark:border-slate-800 shrink-0">
              <div className="flex items-center gap-2">
                <Banknote size={14} className="text-slate-400 shrink-0" />
                <input
                  type="text"
                  value={cashReceived}
                  onChange={e => setCashReceived(e.target.value)}
                  placeholder="Tiền khách đưa"
                  className="flex-1 px-2.5 py-1.5 text-xs border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              {cashReceived && cashChange >= 0 && (
                <p className="text-xs text-emerald-600 dark:text-emerald-400 font-bold mt-1.5 px-1">
                  Tiền thối: {formatCurrency(cashChange)}
                </p>
              )}
              {cashReceived && cashChange < 0 && (
                <p className="text-xs text-red-500 font-bold mt-1.5 px-1">
                  Tiền thiếu: {formatCurrency(Math.abs(cashChange))}
                </p>
              )}
            </div>
          )}

          {/* Success State */}
          {showSuccess && (
            <div className="mx-3 mb-2 flex items-center gap-2 px-3 py-2.5 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-800 rounded-xl text-emerald-600 dark:text-emerald-400">
              <CheckCircle2 size={16} className="shrink-0" />
              <div className="flex flex-col">
                <span className="text-xs font-bold">Đơn hàng đã được tạo!</span>
                {lastOrderTime && (
                  <span className="text-[10px] text-emerald-500 dark:text-emerald-500">
                    Lúc {lastOrderTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false })}
                    &nbsp;—&nbsp;{lastOrderTime.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })}
                  </span>
                )}
              </div>
            </div>
          )}

          {/* Last order timestamp (persistent) */}
          {!showSuccess && lastOrderTime && (
            <div className="mx-3 mb-2 flex items-center gap-2 px-3 py-1.5 bg-slate-50 dark:bg-slate-800/50 border border-slate-200 dark:border-slate-700 rounded-xl">
              <Clock size={12} className="text-slate-400 shrink-0" />
              <span className="text-[10px] text-slate-400">
                Đơn cuối lúc&nbsp;
                <span className="font-bold text-slate-600 dark:text-slate-300">
                  {lastOrderTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false })}
                </span>
                &nbsp;—&nbsp;{lastOrderTime.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })}
              </span>
            </div>
          )}

          {/* Process button */}
          <div className="px-3 pb-3 pt-2 shrink-0">
            <button
              onClick={handleProcessPayment}
              disabled={cart.length === 0 || isProcessing}
              className={cn(
                'w-full py-3.5 rounded-xl font-black text-sm flex items-center justify-center gap-2 transition-all',
                cart.length > 0 && !isProcessing
                  ? 'bg-emerald-600 hover:bg-emerald-500 text-white shadow-lg shadow-emerald-500/20 active:scale-[0.98]'
                  : 'bg-slate-200 dark:bg-slate-800 text-slate-400 cursor-not-allowed'
              )}
            >
              {isProcessing
                ? <><Loader2 size={16} className="animate-spin" /> Đang xử lý...</>
                : <><ChevronRight size={16} /> THANH TOÁN <span className="ml-1 text-emerald-200 text-xs font-mono">(F10)</span></>
              }
            </button>
          </div>
        </aside>
      </div>
    </div>
  );
};

export default POSPage;
