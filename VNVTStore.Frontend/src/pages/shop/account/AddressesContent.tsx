import { useState, useEffect } from 'react';
import { Button } from '@/components/ui';
import { userService, type AddressDto } from '@/services/userService';

const AddressesContent = () => {
    const [addresses, setAddresses] = useState<AddressDto[]>([]);

    useEffect(() => {
        userService.getMyAddresses().then(res => {
            if (res.success && res.data) setAddresses(res.data);
        });
    }, []);

    return (
        <div className="bg-primary rounded-xl p-6">
            <h2 className="text-xl font-bold mb-4">Địa chỉ giao hàng</h2>
            {addresses.length === 0 ? (
                <p className="text-secondary">Chưa có địa chỉ nào được lưu.</p>
            ) : (
                <div className="space-y-4">
                    {addresses.map((addr) => (
                        <div key={addr.code} className="p-4 border rounded-lg">
                            <p className="font-bold">{addr.fullName} <span className="text-sm font-normal text-tertiary">({addr.category})</span></p>
                            <p>{addr.street}, {addr.ward}</p>
                            <p>{addr.district}, {addr.city}</p>
                            <p className="text-sm text-secondary">{addr.phone}</p>
                        </div>
                    ))}
                </div>
            )}
            <Button className="mt-4">Thêm địa chỉ mới</Button>
        </div>
    );
};

export default AddressesContent;
