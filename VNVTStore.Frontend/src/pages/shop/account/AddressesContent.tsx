import { useState, useEffect } from 'react';
import { Button } from '@/components/ui';
import { addressService, type AddressDto, type CreateAddressRequest } from '@/services/userService';
import { useToast } from '@/store';
import { X, MapPin, Star, Edit2, Trash2 } from 'lucide-react';

interface AddressFormData {
    fullName: string;
    phone: string;
    category: string;
    street: string;
    ward: string;
    district: string;
    city: string;
    isDefault: boolean;
}

const emptyForm: AddressFormData = {
    fullName: '',
    phone: '',
    category: 'Nhà riêng',
    street: '',
    ward: '',
    district: '',
    city: '',
    isDefault: false
};

const AddressesContent = () => {
    const toast = useToast();
    const [addresses, setAddresses] = useState<AddressDto[]>([]);
    const [showModal, setShowModal] = useState(false);
    const [editingCode, setEditingCode] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [form, setForm] = useState<AddressFormData>(emptyForm);

    const fetchAddresses = async () => {
        const res = await addressService.search({});
        if (res.success && res.data) {
            setAddresses(res.data.items || []);
        }
    };

    useEffect(() => {
        fetchAddresses();
    }, []);

    // Lock body scroll when modal is open
    useEffect(() => {
        if (showModal) {
            document.body.classList.add('overflow-hidden');
        } else {
            document.body.classList.remove('overflow-hidden');
        }
        return () => {
            document.body.classList.remove('overflow-hidden');
        };
    }, [showModal]);

    const openAddModal = () => {
        setForm(emptyForm);
        setEditingCode(null);
        setShowModal(true);
    };

    const openEditModal = (addr: AddressDto) => {
        // Try to parse addressLine back to components if possible
        // Format assumption: Street, Ward, District
        const parts = (addr.addressLine || '').split(',').map(p => p.trim());
        const street = parts[0] || '';
        const ward = parts.length > 2 ? parts[1] : '';
        const district = parts.length > 2 ? parts[2] : (parts[1] || '');

        setForm({
            fullName: '', // Backend doesn't support yet
            phone: '',    // Backend doesn't support yet
            category: 'Nhà riêng',
            street: street,
            ward: ward,
            district: district,
            city: addr.city || '',
            isDefault: addr.isDefault || false
        });
        setEditingCode(addr.code);
        setShowModal(true);
    };

    const handleSubmit = async () => {
        if (!form.street || !form.city) {
            toast.error('Vui lòng điền địa chỉ và thành phố');
            return;
        }

        const addressPayload: CreateAddressRequest = {
            addressLine: [form.street, form.ward, form.district].filter(Boolean).join(', '),
            city: form.city,
            state: '', 
            postalCode: '',
            country: 'Vietnam',
            isDefault: form.isDefault
        };

        setLoading(true);
        try {
            if (editingCode) {
                await addressService.update(editingCode, addressPayload);
                toast.success('Cập nhật địa chỉ thành công');
            } else {
                await addressService.create(addressPayload);
                toast.success('Thêm địa chỉ mới thành công');
            }
            setShowModal(false);
            fetchAddresses();
        } catch (e) {
            console.error(e);
            toast.error('Có lỗi xảy ra');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (code: string) => {
        if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) return;
        
        try {
            await addressService.delete(code);
            toast.success('Đã xóa địa chỉ');
            fetchAddresses();
        } catch (e) {
            console.error(e);
            toast.error('Không thể xóa địa chỉ');
        }
    };

    const setAsDefault = async (addr: AddressDto) => {
        try {
            // Need to pass full object for update if PUT ignores nulls, 
            // but here we just want to set default. 
            // addressService.update implementation might need checking but typically partials work.
            // Safe way: pass addressLine and city back.
            await addressService.update(addr.code, { 
                addressLine: addr.addressLine,
                city: addr.city,
                isDefault: true 
            });
            toast.success('Đã đặt làm địa chỉ mặc định');
            fetchAddresses();
        } catch (e) {
            console.error(e);
            toast.error('Không thể đặt địa chỉ mặc định');
        }
    };

    return (
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-bold text-gray-800">Địa chỉ giao hàng</h2>
                <Button onClick={openAddModal} variant="primary" size="sm">+ Thêm địa chỉ mới</Button>
            </div>
            
            {addresses.length === 0 ? (
                <div className="text-gray-500 text-center py-8 bg-gray-50 rounded-lg">
                    Chưa có địa chỉ nào được lưu.
                </div>
            ) : (
                <div className="space-y-4">
                    {addresses.map((addr) => (
                        <div key={addr.code} className={`p-4 border rounded-xl relative transition-all duration-200 ${addr.isDefault ? 'border-primary/50 bg-primary/5 shadow-md' : 'hover:border-gray-300'}`}>
                            {addr.isDefault && (
                                <span className="absolute top-3 right-3 bg-primary text-white text-[10px] font-bold px-2 py-1 rounded-full flex items-center gap-1 shadow-sm">
                                    <Star className="w-3 h-3 fill-current" /> Mặc định
                                </span>
                            )}
                            <div className="flex items-start gap-3">
                                <div className={`mt-1 p-2 rounded-full ${addr.isDefault ? 'bg-white text-primary' : 'bg-gray-100 text-gray-500'}`}>
                                    <MapPin className="w-5 h-5 flex-shrink-0" />
                                </div>
                                <div className="flex-1">
                                    <p className="font-bold text-gray-800 text-lg">
                                        {addr.addressLine}
                                    </p>
                                    <p className="text-gray-600 font-medium">{addr.city}</p>
                                    
                                    <div className="flex gap-4 mt-3">
                                        <button
                                            onClick={() => openEditModal(addr)}
                                            className="text-sm text-primary font-medium hover:underline flex items-center gap-1 transition-colors"
                                        >
                                            <Edit2 className="w-3.5 h-3.5" /> Sửa
                                        </button>
                                        <button
                                            onClick={() => handleDelete(addr.code)}
                                            className="text-sm text-red-500 font-medium hover:underline flex items-center gap-1 transition-colors"
                                        >
                                            <Trash2 className="w-3.5 h-3.5" /> Xóa
                                        </button>
                                        {!addr.isDefault && (
                                            <button
                                                onClick={() => setAsDefault(addr)}
                                                className="text-sm text-gray-500 font-medium hover:text-primary hover:underline flex items-center gap-1 transition-colors"
                                            >
                                                <Star className="w-3.5 h-3.5" /> Đặt mặc định
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Add/Edit Modal */}
            {showModal && (
                <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
                    {/* Backdrop */}
                    <div 
                        className="absolute inset-0 bg-black/40 backdrop-blur-sm transition-opacity"
                        onClick={() => setShowModal(false)}
                    />
                    
                    {/* Modal Content - Scaled Down */}
                    <div className="bg-white rounded-2xl w-full max-w-[420px] max-h-[85vh] overflow-y-auto shadow-2xl relative z-10 animate-in fade-in zoom-in-95 duration-200">
                        <div className="sticky top-0 bg-white px-5 py-4 border-b flex justify-between items-center z-10 sticky-header">
                            <h3 className="text-lg font-bold text-gray-800">
                                {editingCode ? 'Chỉnh sửa địa chỉ' : 'Thêm địa chỉ mới'}
                            </h3>
                            <button 
                                onClick={() => setShowModal(false)}
                                className="p-1.5 hover:bg-gray-100 rounded-full text-gray-500 transition-colors"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        <div className="p-5 space-y-4">
                            {/* Note: FullName and Phone are purely UI only usage here for user comfort */}
                            <div className="grid grid-cols-2 gap-3">
                                <div>
                                    <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Họ tên người nhận</label>
                                    <input
                                        type="text"
                                        value={form.fullName}
                                        onChange={(e) => setForm({...form, fullName: e.target.value})}
                                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                        placeholder="Nguyễn Văn A"
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Số điện thoại</label>
                                    <input
                                        type="tel"
                                        value={form.phone}
                                        onChange={(e) => setForm({...form, phone: e.target.value})}
                                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                        placeholder="0912..."
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Loại địa chỉ</label>
                                <div className="flex gap-2">
                                    {['Nhà riêng', 'Văn phòng', 'Khác'].map(cat => (
                                        <button
                                            key={cat}
                                            onClick={() => setForm({...form, category: cat})}
                                            className={`flex-1 py-1.5 text-sm rounded-lg border transition-all ${
                                                form.category === cat 
                                                ? 'bg-primary/10 border-primary text-primary font-medium' 
                                                : 'border-gray-200 text-gray-600 hover:bg-gray-50'
                                            }`}
                                        >
                                            {cat}
                                        </button>
                                    ))}
                                </div>
                            </div>
                            
                            <hr className="border-dashed border-gray-200" />
                            
                            <div>
                                <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Địa chỉ cụ thể *</label>
                                <input
                                    type="text"
                                    value={form.street}
                                    onChange={(e) => setForm({...form, street: e.target.value})}
                                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                    placeholder="Số nhà, tên đường..."
                                />
                            </div>
                            <div className="grid grid-cols-2 gap-3">
                                <div>
                                    <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Phường/Xã</label>
                                    <input
                                        type="text"
                                        value={form.ward}
                                        onChange={(e) => setForm({...form, ward: e.target.value})}
                                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Quận/Huyện</label>
                                    <input
                                        type="text"
                                        value={form.district}
                                        onChange={(e) => setForm({...form, district: e.target.value})}
                                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                    />
                                </div>
                            </div>
                            <div>
                                <label className="block text-xs font-semibold text-gray-600 mb-1.5 uppercase">Tỉnh/Thành phố *</label>
                                <input
                                    type="text"
                                    value={form.city}
                                    onChange={(e) => setForm({...form, city: e.target.value})}
                                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                                    placeholder="TP. Hồ Chí Minh"
                                />
                            </div>
                            
                            <div className="pt-2">
                                <label className="flex items-center gap-2 cursor-pointer group">
                                    <div className={`w-4 h-4 rounded border flex items-center justify-center transition-colors ${form.isDefault ? 'bg-primary border-primary' : 'border-gray-300 group-hover:border-primary'}`}>
                                        {form.isDefault && <div className="w-2 h-2 bg-white rounded-sm" />}
                                    </div>
                                    <input
                                        type="checkbox"
                                        checked={form.isDefault}
                                        onChange={(e) => setForm({...form, isDefault: e.target.checked})}
                                        className="hidden"
                                    />
                                    <span className="text-sm font-medium text-gray-700 group-hover:text-primary transition-colors">Đặt làm địa chỉ mặc định</span>
                                </label>
                            </div>
                        </div>

                        <div className="p-5 border-t bg-gray-50 rounded-b-2xl flex gap-3">
                            <Button 
                                variant="secondary" 
                                onClick={() => setShowModal(false)} 
                                className="flex-1 bg-white border border-gray-200 hover:bg-gray-100 hover:text-gray-900"
                            >
                                Hủy bỏ
                            </Button>
                            <Button 
                                onClick={handleSubmit} 
                                isLoading={loading} 
                                className="flex-1 shadow-lg shadow-primary/30"
                            >
                                {editingCode ? 'Lưu thay đổi' : 'Thêm địa chỉ'}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default AddressesContent;
