import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { addressService, type AddressDto, type CreateAddressRequest } from '@/services/userService';
import { useToast } from '@/store';
import { MapPin, Star, Edit2, Trash2, CheckCircle, Plus } from 'lucide-react';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { BaseForm, type FieldGroup } from '@/components/common/BaseForm';

const AddressesContent = () => {
    const { t } = useTranslation();
    const { addressSchema } = createSchemas(t);
    type AddressFormData = z.infer<typeof addressSchema>;

    const emptyForm: AddressFormData = {
        fullName: '',
        phone: '',
        category: t('addresses.form.options.home'),
        street: '',
        ward: '',
        district: '',
        city: '',
        isDefault: false
    };

    const toast = useToast();
    const [addresses, setAddresses] = useState<AddressDto[]>([]);
    const [showModal, setShowModal] = useState(false);
    const [showConfirmModal, setShowConfirmModal] = useState(false);
    const [addressToDelete, setAddressToDelete] = useState<string | null>(null);
    const [editingCode, setEditingCode] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [tempFormData, setTempFormData] = useState<AddressFormData | null>(null);

    const fetchAddresses = async () => {
        const res = await addressService.search({});
        if (res.success && res.data) {
            setAddresses(res.data.items || []);
        }
    };

    useEffect(() => {
        fetchAddresses();
    }, []);

    const openAddModal = () => {
        setTempFormData(emptyForm);
        setEditingCode(null);
        setShowModal(true);
    };

    const openEditModal = (addr: AddressDto) => {
        const parts = (addr.addressLine || '').split(',').map((p: string) => p.trim());
        const street = parts[0] || '';
        const ward = parts.length > 2 ? parts[1] : '';
        const district = parts.length > 2 ? parts[2] : (parts[1] || '');

        setTempFormData({
            fullName: addr.fullName || '',
            phone: addr.phone || '',
            category: addr.category || t('addresses.form.options.home'),
            street: street,
            ward: ward,
            district: district,
            city: addr.city || '',
            isDefault: addr.isDefault || false
        });
        setEditingCode(addr.code);
        setShowModal(true);
    };

    const handleFormSubmit = (data: AddressFormData) => {
        setTempFormData(data);
        setShowConfirmModal(true);
    };

    const onConfirmSubmit = async () => {
        if (!tempFormData) return;
        
        const addressPayload: CreateAddressRequest = {
            fullName: tempFormData.fullName,
            phone: tempFormData.phone,
            category: tempFormData.category,
            addressLine: [tempFormData.street, tempFormData.ward, tempFormData.district].filter((p: string | undefined): p is string => !!p?.trim()).join(', '),
            city: tempFormData.city,
            state: '', 
            postalCode: '',
            country: 'Vietnam',
            isDefault: tempFormData.isDefault
        };

        setLoading(true);
        try {
            if (editingCode) {
                await addressService.update(editingCode, addressPayload);
                toast.success(t('addresses.messages.updateSuccess'));
            } else {
                await addressService.create(addressPayload);
                toast.success(t('addresses.messages.createSuccess'));
            }
            fetchAddresses();
            setShowModal(false);
            setShowConfirmModal(false);
        } catch (e) {
            console.error(e);
            toast.error(t('addresses.messages.saveError'));
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (code: string) => {
        setAddressToDelete(code);
    };

    const confirmDelete = async () => {
        if (!addressToDelete) return;
        
        setLoading(true);
        try {
            await addressService.delete(addressToDelete);
            toast.success(t('addresses.messages.deleteSuccess'));
            fetchAddresses();
            setAddressToDelete(null);
        } catch (e) {
            console.error(e);
            toast.error(t('addresses.messages.deleteError'));
        } finally {
            setLoading(false);
        }
    };

    const setAsDefault = async (addr: AddressDto) => {
        try {
            await addressService.update(addr.code, { 
                fullName: addr.fullName,
                phone: addr.phone,
                category: addr.category,
                addressLine: addr.addressLine,
                city: addr.city,
                isDefault: true 
            });
            toast.success(t('addresses.messages.setDefaultSuccess'));
            fetchAddresses();
        } catch (e) {
            console.error(e);
            toast.error(t('addresses.messages.saveError'));
        }
    };

    const fieldGroups: FieldGroup[] = useMemo(() => [
        {
            title: t('addresses.form.groups.contact'),
            fields: [
                { name: 'fullName', type: 'text', label: t('addresses.form.labels.fullName'), required: true, colSpan: 12, placeholder: t('addresses.form.placeholders.fullName'), size: 'lg' },
                { name: 'phone', type: 'phone', label: t('addresses.form.labels.phone'), required: true, colSpan: 12, placeholder: t('common.placeholders.phone'), size: 'lg' },
                { 
                    name: 'category', 
                    type: 'select', 
                    label: t('addresses.form.labels.category'), 
                    required: true, 
                    colSpan: 12,
                    size: 'lg',
                    options: [
                        { value: 'Nhà riêng', label: t('addresses.form.options.home', 'Nhà riêng') },
                        { value: 'Văn phòng', label: t('addresses.form.options.office', 'Văn phòng') },
                        { value: 'Khác', label: t('addresses.form.options.other', 'Khác') },
                    ]
                },
            ]
        },
        {
            title: t('addresses.form.groups.area'),
            fields: [
                { name: 'city', type: 'text', label: t('addresses.form.labels.city'), required: true, colSpan: 12, placeholder: t('addresses.form.placeholders.city'), size: 'lg' },
                { name: 'district', type: 'text', label: t('addresses.form.labels.district'), required: true, colSpan: 6, size: 'lg' },
                { name: 'ward', type: 'text', label: t('addresses.form.labels.ward'), colSpan: 6, size: 'lg' },
            ]
        },
        {
            title: t('addresses.form.groups.details'),
            fields: [
                { name: 'street', type: 'textarea', label: t('addresses.form.labels.street'), required: true, colSpan: 12, rows: 4, placeholder: t('addresses.form.placeholders.street'), size: 'lg' },
                { name: 'isDefault', type: 'switch', label: t('addresses.form.labels.isDefault'), description: t('addresses.form.labels.isDefaultDesc'), colSpan: 12 },
            ]
        }
    ], [t]);

    return (
        <div className="bg-white rounded-2xl shadow-sm p-8 border border-gray-100 dark:bg-slate-900 dark:border-slate-800">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900 dark:text-white uppercase tracking-tight">{t('addresses.title')}</h2>
                    <p className="text-gray-500 dark:text-gray-400 mt-1">{t('addresses.subtitle')}</p>
                </div>
                <Button 
                    variant="primary" 
                    leftIcon={<Plus size={18} />} 
                    onClick={() => {
                        setEditingCode(null);
                        setTempFormData(emptyForm);
                        setShowModal(true);
                    }}
                    className="shadow-md hover:shadow-lg transition-all"
                >
                    {t('addresses.addNew')}
                </Button>
            </div>
            
            {addresses.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 bg-gray-50/50 dark:bg-slate-800/50 rounded-2xl border-2 border-dashed border-gray-100 dark:border-slate-700">
                    <div className="w-16 h-16 bg-white dark:bg-slate-800 rounded-full flex items-center justify-center shadow-sm mb-4">
                        <MapPin className="w-8 h-8 text-gray-300" />
                    </div>
                    <p className="text-gray-400 font-medium">{t('addresses.noAddresses')}</p>
                    <button onClick={openAddModal} className="mt-2 text-primary text-sm font-semibold hover:underline">
                        {t('addresses.addFirst')}
                    </button>
                </div>
            ) : (
                <div className="grid grid-cols-1 gap-5">
                    {addresses.map((addr) => (
                        <div 
                            key={addr.code} 
                            className={`group p-5 border-2 rounded-2xl relative transition-all duration-300 ${
                                addr.isDefault 
                                ? 'border-primary/20 bg-primary/[0.02] shadow-sm' 
                                : 'border-gray-100 dark:border-slate-800 hover:border-primary/20 hover:bg-white dark:hover:bg-slate-800/50 hover:shadow-md'
                            }`}
                        >
                            <div className="flex items-start gap-4">
                                <div className={`p-3 rounded-xl transition-colors ${
                                    addr.isDefault ? 'bg-primary text-white' : 'bg-gray-100 dark:bg-slate-800 text-gray-400 group-hover:bg-primary/10 group-hover:text-primary'
                                }`}>
                                    <MapPin className="w-6 h-6" />
                                </div>
                                <div className="flex-1 min-w-0">
                                    <div className="flex flex-wrap items-center gap-2 mb-1">
                                        <h3 className="font-bold text-gray-900 dark:text-white text-lg leading-tight truncate">
                                            {addr.fullName || t('common.fields.customer')}
                                        </h3>
                                        <span className="hidden md:inline text-gray-300 dark:text-slate-700">|</span>
                                        <span className="text-gray-600 dark:text-slate-400 font-medium">{addr.phone || 'N/A'}</span>
                                        {addr.isDefault && (
                                            <span className="bg-primary/10 text-primary text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider flex items-center gap-1">
                                                <Star className="w-2.5 h-2.5 fill-current" /> {t('addresses.defaultBadge')}
                                            </span>
                                        )}
                                        <span className="bg-gray-100 dark:bg-slate-800 text-gray-500 text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
                                            {addr.category}
                                        </span>
                                    </div>
                                    <p className="text-gray-600 dark:text-slate-400 text-sm leading-relaxed mb-4">
                                        {addr.addressLine}, {addr.city}
                                    </p>
                                    
                                    <div className="flex flex-wrap gap-4 pt-3 border-t border-gray-50 dark:border-slate-800">
                                        <button
                                            onClick={() => openEditModal(addr)}
                                            className="text-sm text-primary font-bold hover:opacity-80 flex items-center gap-1.5 transition-all"
                                        >
                                            <Edit2 className="w-4 h-4" /> {t('common.actions.edit')}
                                        </button>
                                        <button
                                            onClick={() => handleDelete(addr.code)}
                                            className="text-sm text-red-500 font-bold hover:opacity-80 flex items-center gap-1.5 transition-all"
                                        >
                                            <Trash2 className="w-4 h-4" /> {t('common.actions.delete')}
                                        </button>
                                        {!addr.isDefault && (
                                            <button
                                                onClick={() => setAsDefault(addr)}
                                                className="text-sm text-gray-400 font-bold hover:text-primary flex items-center gap-1.5 transition-all ml-auto"
                                            >
                                                <Star className="w-4 h-4" /> {t('addresses.setDefault')}
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Address Form Modal using BaseForm */}
            <BaseForm<AddressFormData>
                isModal
                modalOpen={showModal}
                modalTitle={editingCode ? t('addresses.form.titleEdit') : t('addresses.form.titleAdd')}
                modalSize="6xl"
                onModalClose={() => setShowModal(false)}
                schema={addressSchema}
                defaultValues={tempFormData || emptyForm}
                fieldGroups={fieldGroups}
                groupLayoutClassName="grid grid-cols-1 lg:grid-cols-3 gap-8"
                onSubmit={handleFormSubmit}
                onCancel={() => setShowModal(false)}
                submitLabel={editingCode ? t('addresses.form.submitEdit') : t('addresses.form.submitAdd')}
                cancelLabel={t('common.actions.back')}
                isLoading={loading}
            />

            {/* Save Confirmation Dialog */}
            <ConfirmDialog
                isOpen={showConfirmModal}
                onClose={() => setShowConfirmModal(false)}
                onConfirm={onConfirmSubmit}
                title={t('addresses.confirm.saveTitle')}
                message={t('addresses.confirm.saveMessage')}
                variant="success"
                confirmText={t('addresses.confirm.saveButton')}
                isLoading={loading}
                icon={<CheckCircle className="size-6 text-green-600" />}
            />

            {/* Delete Confirmation Dialog */}
            <ConfirmDialog
                isOpen={!!addressToDelete}
                onClose={() => setAddressToDelete(null)}
                onConfirm={confirmDelete}
                title={t('addresses.confirm.deleteTitle')}
                message={t('addresses.confirm.deleteMessage')}
                variant="danger"
                confirmText={t('addresses.confirm.deleteButton')}
                isLoading={loading}
            />
        </div>
    );
};

export default AddressesContent;
