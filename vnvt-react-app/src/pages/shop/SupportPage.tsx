import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import {
    HelpCircle, MessageCircle, Phone, Mail, FileText,
    ChevronDown, ChevronUp, Package, CreditCard, Truck, RefreshCw
} from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { Link } from 'react-router-dom';

export const SupportPage = () => {
    const { t } = useTranslation();
    const [openFaq, setOpenFaq] = useState<number | null>(0);

    const supportCategories = [
        { icon: Package, title: t('support.orderIssues'), link: '/contact' },
        { icon: CreditCard, title: t('support.paymentHelp'), link: '/contact' },
        { icon: Truck, title: t('support.shippingInfo'), link: '/tracking' },
        { icon: RefreshCw, title: t('support.returnsPolicy'), link: '/contact' },
    ];

    const faqs = [
        { q: t('support.faq1q'), a: t('support.faq1a') },
        { q: t('support.faq2q'), a: t('support.faq2a') },
        { q: t('support.faq3q'), a: t('support.faq3a') },
        { q: t('support.faq4q'), a: t('support.faq4a') },
    ];

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">🛟 {t('support.title')}</h1>
                    <p className="text-secondary text-lg max-w-xl mx-auto">{t('support.subtitle')}</p>
                </motion.div>

                {/* Quick Links */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-12">
                    {supportCategories.map((cat, index) => (
                        <motion.div
                            key={index}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.1 }}
                        >
                            <Link
                                to={cat.link}
                                className="bg-primary rounded-xl p-6 text-center block hover:shadow-md transition-shadow"
                            >
                                <cat.icon className="mx-auto mb-3 text-indigo-600" size={32} />
                                <p className="font-medium">{cat.title}</p>
                            </Link>
                        </motion.div>
                    ))}
                </div>

                <div className="grid lg:grid-cols-2 gap-12">
                    {/* FAQs */}
                    <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }}>
                        <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
                            <HelpCircle size={24} />
                            {t('support.faqTitle')}
                        </h2>
                        <div className="space-y-3">
                            {faqs.map((faq, index) => (
                                <div key={index} className="bg-primary rounded-xl overflow-hidden">
                                    <button
                                        onClick={() => setOpenFaq(openFaq === index ? null : index)}
                                        className="w-full flex items-center justify-between p-4 text-left"
                                    >
                                        <span className="font-medium">{faq.q}</span>
                                        {openFaq === index ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
                                    </button>
                                    {openFaq === index && (
                                        <div className="px-4 pb-4 text-secondary text-sm">
                                            {faq.a}
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </motion.div>

                    {/* Contact Options */}
                    <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}>
                        <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
                            <MessageCircle size={24} />
                            {t('support.contactTitle')}
                        </h2>
                        <div className="space-y-4">
                            <div className="bg-primary rounded-xl p-6">
                                <div className="flex items-center gap-4 mb-4">
                                    <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                                        <Phone className="text-green-600" size={24} />
                                    </div>
                                    <div>
                                        <p className="font-medium">{t('support.hotline')}</p>
                                        <p className="text-xl font-bold text-green-600">1900 123 456</p>
                                    </div>
                                </div>
                                <p className="text-sm text-tertiary">{t('support.hotlineHours')}</p>
                            </div>

                            <div className="bg-primary rounded-xl p-6">
                                <div className="flex items-center gap-4 mb-4">
                                    <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                                        <Mail className="text-blue-600" size={24} />
                                    </div>
                                    <div>
                                        <p className="font-medium">Email</p>
                                        <p className="text-blue-600">support@vnvtstore.com</p>
                                    </div>
                                </div>
                                <p className="text-sm text-tertiary">{t('support.emailResponse')}</p>
                            </div>

                            <Link to="/contact">
                                <Button fullWidth className="mt-4">
                                    {t('support.sendMessage')}
                                </Button>
                            </Link>
                        </div>
                    </motion.div>
                </div>
            </div>
        </div>
    );
};

export default SupportPage;
