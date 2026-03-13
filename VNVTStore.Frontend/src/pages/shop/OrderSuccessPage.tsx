import { useEffect, useState } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Check, Printer, Cloud } from 'lucide-react';
import { Button } from '@/components/ui';
import { orderService, type OrderDto } from '@/services/orderService';
import { formatCurrency } from '@/utils/format';
import CustomImage from '@/components/common/Image';

import { useSEO } from '@/hooks/useSEO';

const OrderSuccessPage = () => {
     
  useTranslation();
  
  useSEO({
    title: 'Đặt hàng thành công',
    noindex: true,
  });
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const orderCode = searchParams.get('code');
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (orderCode) {
        orderService.getByCode(orderCode)
            .then(res => {
                if (res.success && res.data) {
                    setOrder(res.data);
                } else {
                    // Logic failed (not found) -> Redirect
                    navigate('/products');
                }
            })
            .catch(err => {
                console.error(err);
                navigate('/products');
            })
            .finally(() => setLoading(false));
    } else {
        // No code -> Redirect
        navigate('/products');
    }
  }, [orderCode, navigate]);

  if (loading) return (
    <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
    </div>
  );

  if (!order) return null; // Should have redirected

  // Helper to get formatted date if we wanted to show it
  // const orderDate = new Date(order.createdAt).toLocaleDateString('vi-VN');

  return (
    <div className="min-h-screen bg-[#F4F6F8] font-sans">
      <div className="container mx-auto px-4 py-8 max-w-6xl">
        
        {/* Top Cloud Icon (mimicking the screenshot's top icon) */}
        <div className="flex justify-center mb-6">
            <Cloud size={48} className="text-gray-800" strokeWidth={1.5} />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
            {/* Left Content Area */}
            <div className="lg:col-span-7">
                <div className="flex flex-col sm:flex-row items-start gap-5 mb-8 text-left">
                     <div className="w-20 h-20 rounded-full border-[3px] border-[#87D068] flex-shrink-0 flex items-center justify-center bg-white">
                        <Check className="text-[#87D068]" size={40} strokeWidth={3} />
                     </div>
                     <div className="mt-2">
                        <h1 className="text-2xl font-bold text-[#333333] mb-2">Cảm ơn bạn đã đặt hàng</h1>
                        <p className="text-[#666666] text-sm leading-relaxed">
                            Một email xác nhận đã được gửi tới <span className="text-[#333333]">{order.userCode || 'email của bạn'}</span>.
                            <br/>Xin vui lòng kiểm tra email của bạn
                        </p>
                     </div>
                </div>

                {/* Gray Box for Info */}
                <div className="bg-[#F0F2F5] p-6 sm:p-8 rounded-sm grid grid-cols-1 md:grid-cols-2 gap-y-8 gap-x-8 text-left border border-gray-100">
                     {/* Buyer Info */}
                    <div>
                        <h3 className="text-[15px] font-medium text-[#333333] mb-2">Thông tin mua hàng</h3>
                        <div className="text-[#666666] text-[13px] space-y-1">
                            <p>{order.shippingName || 'Khách lẻ'}</p>
                            <p>{order.userCode}</p>
                        </div>
                    </div>

                    {/* Shipping Address */}
                    <div>
                        <h3 className="text-[15px] font-medium text-[#333333] mb-2">Địa chỉ nhận hàng</h3>
                        <div className="text-[#666666] text-[13px] space-y-1">
                            <p>{order.shippingName}</p>
                            <p>{order.shippingAddress}</p>
                            <p>{order.city}</p> 
                        </div>
                    </div>

                    {/* Payment Method */}
                    <div>
                        <h3 className="text-[15px] font-medium text-[#333333] mb-2">Phương thức thanh toán</h3>
                        <div className="text-[#666666] text-[13px]">
                             <p>{order.paymentMethod === 'COD' ? 'Thanh toán khi giao hàng (COD)' : order.paymentMethod}</p>
                        </div>
                    </div>

                    {/* Shipping Method */}
                    <div>
                        <h3 className="text-[15px] font-medium text-[#333333] mb-2">Phương thức vận chuyển</h3>
                        <div className="text-[#666666] text-[13px]">
                            <p>{order.shippingFee === 0 ? 'FREE SHIP' : 'Giao hàng tiêu chuẩn'}</p>
                        </div>
                    </div>
                </div>

                {/* Actions */}
                <div className="flex flex-col-reverse sm:flex-row items-center justify-between sm:justify-end gap-6 mt-10">
                     
                     <Link to="/" className="w-full sm:w-auto">
                        <Button className="w-full sm:w-auto h-12 px-10 text-[15px] bg-[#337AB7] hover:bg-[#286090] text-white font-medium rounded-[4px] shadow-sm transition-colors">
                            Tiếp tục mua hàng
                        </Button>
                    </Link>
                    
                    <button 
                        onClick={() => window.print()} 
                        className="flex items-center gap-2 text-[#337AB7] hover:text-[#23527c] font-medium transition-colors"
                    >
                        <Printer size={20} />
                        <span className="text-[15px]">In</span>
                    </button>
                </div>
            </div>

            {/* Right Sidebar: Order Summary */}
            <div className="lg:col-span-5">
                <div className="bg-white rounded-sm shadow-sm border border-gray-200 overflow-hidden">
                    <div className="px-5 py-3 border-b border-gray-100 bg-white">
                        <h3 className="font-bold text-[#333333] text-[15px]">Đơn hàng #{order.code} ({order.orderItems?.length})</h3>
                    </div>
                    <div className="p-5">
                        <div className="space-y-4 mb-4 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
                            {order.orderItems?.map((item, idx) => (
                                <div key={idx} className="flex gap-4">
                                    <div className="relative w-14 h-14 rounded-[4px] border border-gray-200 overflow-hidden flex-shrink-0 bg-gray-50">
                                         <CustomImage 
                                            src={item.productImage} 
                                            alt={item.productName}
                                            className="w-full h-full object-cover" 
                                        />
                                        <div className="absolute -top-2 -right-2 bg-[#8F8F8F] text-white text-[11px] w-5 h-5 flex items-center justify-center rounded-full font-bold shadow-sm">
                                            {item.quantity}
                                        </div>
                                    </div>
                                    <div className="flex-1 min-w-0 pt-0.5">
                                        <p className="text-[13px] text-[#333333] font-medium leading-snug line-clamp-2 uppercase">
                                            {item.productName}
                                        </p>
                                        <p className="text-[12px] text-[#999999] mt-0.5">
                                           {/*  VARIANT INFO WOULD GO HERE IF AVAILABLE IN DTO */}
                                        </p> 
                                    </div>
                                    <div className="text-right pt-0.5">
                                        <p className="text-[13px] font-medium text-[#333333]">
                                            {formatCurrency(item.priceAtOrder * item.quantity)}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="border-t border-gray-100 pt-4 space-y-3 text-[14px]">
                            <div className="flex justify-between text-[#666666]">
                                <span>Tạm tính</span>
                                <span className="text-[#333333]">
                                    {formatCurrency(order.totalAmount - (order.shippingFee || 0) + (order.discountAmount || 0))}
                                </span>
                            </div>
                            <div className="flex justify-between text-[#666666]">
                                <span>Phí vận chuyển</span>
                                <span className="text-[#333333]">
                                     {order.shippingFee === 0 ? 'Miễn phí' : formatCurrency(order.shippingFee)}
                                </span>
                            </div>
                             {(order.discountAmount || 0) > 0 && (
                                <div className="flex justify-between text-success">
                                    <span>Giảm giá</span>
                                    <span>-{formatCurrency(order.discountAmount)}</span>
                                </div>
                            )}
                        </div>

                        <div className="border-t border-gray-100 mt-4 pt-4 flex justify-between items-center">
                            <span className="text-[16px] text-[#666666] font-medium">Tổng cộng</span>
                            <span className="text-[20px] font-bold text-[#337AB7]">
                                {formatCurrency(order.finalAmount || order.totalAmount)}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
      </div>
    </div>
  );
};

export default OrderSuccessPage;
