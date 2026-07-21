import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { MapPin, Phone, Mail, Clock, Send } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast } from '@/store';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';
import { useQuery } from '@tanstack/react-query';
import { systemConfigService } from '@/services/systemConfigService';

import { useSEO } from '@/hooks/useSEO';

export const ContactPage = () => {
    const { t } = useTranslation();
    
    // Fetch public contact settings from the Backend
    const { data: contactsRes } = useQuery({
        queryKey: ['public-contacts'],
        queryFn: () => systemConfigService.getPublicContacts(),
        staleTime: 5 * 60 * 1000, // cache for 5 minutes
    });

    const getContactValue = (code: string, fallback: string) => {
        return contactsRes?.data?.[code] || fallback;
    };

    const phone = getContactValue('CONTACT_PHONE', '1900 123 456');
    const contactEmail = getContactValue('ADMIN_EMAIL', 'contact@vnvtstore.com');
    const address = getContactValue('CONTACT_ADDRESS', '123 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh');
    const mapsUrl = getContactValue('CONTACT_MAPS', 'https://maps.google.com/?q=VNVT+Store');
    const zaloNumber = getContactValue('CONTACT_ZALO', '');
    const zaloUrl = zaloNumber ? (zaloNumber.startsWith('http') ? zaloNumber : `https://zalo.me/${zaloNumber}`) : '#';

    useSEO({
        title: 'Liên hệ',
        description: `Liên hệ với VNVT Store. Hotline: ${phone}. Email: ${contactEmail}. Địa chỉ: ${address}.`,
        canonicalPath: '/contact',
    });
    const { contactSchema } = createSchemas(t);
    type ContactFormData = z.infer<typeof contactSchema>;
    const toast = useToast();

    
    const { register, handleSubmit, reset, formState: { errors } } = useForm<ContactFormData>({
        resolver: zodResolver(contactSchema),
        defaultValues: {
            fullName: '',
            email: '',
            phone: '',
            subject: '',
            message: '',
        }
    });

    const onSubmit = () => {
        toast.success(t('contactPage.success'));
        reset();
    };

    const contactInfo = [
        { icon: MapPin, title: t('contactPage.info.address'), content: address },
        { icon: Phone, title: t('contactPage.info.hotline'), content: phone },
        { icon: Mail, title: t('contactPage.info.email'), content: contactEmail },
        { icon: Clock, title: t('contactPage.info.workingHours'), content: '8:00 - 21:00 (Thứ 2 - CN)' },
    ];


    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">{t('contactPage.title')}</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        {t('contactPage.subtitle')}
                    </p>
                </motion.div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
                    {/* Contact Form */}
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        className="bg-primary rounded-2xl p-8 shadow-lg"
                    >
                        <h2 className="text-2xl font-bold mb-6">{t('contactPage.formTitle')}</h2>
                        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <Input
                                    label={t('contactPage.fields.fullName')}
                                    placeholder={t('contactPage.placeholders.fullName')}
                                    {...register('fullName')}
                                    error={errors.fullName?.message}
                                    isRequired
                                />
                                <Input
                                    label={t('contactPage.fields.email')}
                                    type="email"
                                    placeholder={t('contactPage.placeholders.email')}
                                    {...register('email')}
                                    error={errors.email?.message}
                                    isRequired
                                />
                            </div>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <Input
                                    label={t('contactPage.fields.phone')}
                                    placeholder={t('contactPage.placeholders.phone')}
                                    {...register('phone')}
                                    error={errors.phone?.message}
                                />
                                <Input
                                    label={t('contactPage.fields.subject')}
                                    placeholder={t('contactPage.placeholders.subject')}
                                    {...register('subject')}
                                    error={errors.subject?.message}
                                    isRequired
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-2">
                                    {t('contactPage.fields.message')} <span className="text-red-500">*</span>
                                </label>
                                <textarea
                                    rows={5}
                                    placeholder={t('contactPage.placeholders.message')}
                                    {...register('message')}
                                    className={`w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 transition-all resize-none ${errors.message ? 'border-red-500 bg-red-50/50' : 'border-slate-200'}`}
                                />
                                {errors.message && <p className="text-red-500 text-xs mt-1 font-medium">{errors.message.message}</p>}
                            </div>
                            <Button type="submit" fullWidth rightIcon={<Send size={18} />}>
                                {t('contactPage.send')}
                            </Button>
                        </form>
                    </motion.div>

                    {/* Contact Info */}
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        className="space-y-6"
                    >
                        {/* Info Cards */}
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            {contactInfo.map((info, index) => (
                                <div
                                    key={index}
                                    className="bg-primary rounded-xl p-6 shadow-md"
                                >
                                    <div className="w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                                        <info.icon size={24} className="text-primary" />
                                    </div>
                                    <h3 className="font-semibold mb-1">{info.title}</h3>
                                    <p className="text-secondary text-sm">{info.content}</p>
                                </div>
                            ))}
                        </div>

                        {/* Map Placeholder */}
                        {/* Google Map */}
                        <a href={mapsUrl} target="_blank" rel="noopener noreferrer" className="block bg-primary rounded-2xl overflow-hidden shadow-lg h-80 relative group">
                            <iframe 
                                src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.424237106093!2d106.69875427599763!3d10.778782959146187!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f48356f91cb%3A0x62952467d0186196!2sNotre%20Dame%20Cathedral%20of%20Saigon!5e0!3m2!1sen!2s!4v1707557123456!5m2!1sen!2s" 
                                width="100%" 
                                height="100%" 
                                style={{ border: 0, pointerEvents: 'none' }} 
                                allowFullScreen 
                                loading="lazy" 
                                referrerPolicy="no-referrer-when-downgrade"
                                title="Store Location"
                                className="grayscale hover:grayscale-0 transition-all duration-500"
                            ></iframe>
                             <div className="absolute inset-0 pointer-events-none border-4 border-primary/20 rounded-2xl shadow-inner"></div>
                        </a>

                        {/* Social Links */}
                        <div className="bg-primary rounded-xl p-6 shadow-md">
                            <h3 className="font-semibold mb-4">{t('contactPage.socialTitle')}</h3>
                            <div className="flex flex-wrap gap-4">
                                {[
                                    { label: 'Facebook', href: facebookUrl },
                                    { label: 'Zalo', href: zaloUrl },
                                    { label: 'TikTok', href: getSecretValue('CONTACT_TIKTOK', '#') },
                                    { label: 'Instagram', href: getSecretValue('CONTACT_INSTAGRAM', '#') },
                                    { label: 'Youtube', href: getSecretValue('CONTACT_YOUTUBE', '#') }
                                ].map((social) => (
                                    <a
                                        key={social.label}
                                        href={social.href}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="px-4 py-2 bg-secondary hover:bg-hover rounded-lg text-sm font-medium transition-colors"
                                    >
                                        {social.label}
                                    </a>
                                ))}
                            </div>
                        </div>
                    </motion.div>
                </div>
            </div>
        </div>
    );
};

export default ContactPage;
