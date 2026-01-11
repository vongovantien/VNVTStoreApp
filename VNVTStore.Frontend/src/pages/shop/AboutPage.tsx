import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Award, Users, Target, Heart, ShieldCheck, Truck, CheckCircle } from 'lucide-react';

export const AboutPage = () => {
    const { t } = useTranslation();

    const stats = [
        { value: '10+', label: t('about.yearsExperience') },
        { value: '50K+', label: t('about.happyCustomers') },
        { value: '1000+', label: t('about.products') },
        { value: '24/7', label: t('about.support') },
    ];

    const values = [
        { icon: Award, title: t('about.qualityTitle'), desc: t('about.qualityDesc') },
        { icon: Heart, title: t('about.customerTitle'), desc: t('about.customerDesc') },
        { icon: ShieldCheck, title: t('about.trustTitle'), desc: t('about.trustDesc') },
        { icon: Truck, title: t('about.deliveryTitle'), desc: t('about.deliveryDesc') },
    ];

    return (
        <div className="min-h-screen bg-secondary">
            {/* Hero */}
            <div className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white py-20">
                <div className="container mx-auto px-4 text-center">
                    <motion.h1
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="text-4xl md:text-5xl font-bold mb-4"
                    >
                        {t('about.title')}
                    </motion.h1>
                    <motion.p
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.1 }}
                        className="text-xl text-white/80 max-w-2xl mx-auto"
                    >
                        {t('about.subtitle')}
                    </motion.p>
                </div>
            </div>

            <div className="container mx-auto px-4 py-16">
                {/* Stats */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-6 mb-16">
                    {stats.map((stat, index) => (
                        <motion.div
                            key={index}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.1 }}
                            className="bg-primary rounded-xl p-6 text-center shadow-md"
                        >
                            <p className="text-3xl font-bold text-indigo-600 mb-2">{stat.value}</p>
                            <p className="text-secondary">{stat.label}</p>
                        </motion.div>
                    ))}
                </div>

                {/* Story */}
                <div className="grid md:grid-cols-2 gap-12 items-center mb-16">
                    <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }}>
                        <h2 className="text-3xl font-bold mb-6">{t('about.storyTitle')}</h2>
                        <p className="text-secondary mb-4">{t('about.storyP1')}</p>
                        <p className="text-secondary">{t('about.storyP2')}</p>
                    </motion.div>
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        className="bg-gradient-to-br from-indigo-100 to-purple-100 rounded-2xl h-80 flex items-center justify-center"
                    >
                        <Users size={120} className="text-indigo-600/30" />
                    </motion.div>
                </div>

                {/* Values */}
                <h2 className="text-3xl font-bold text-center mb-10">{t('about.valuesTitle')}</h2>
                <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
                    {values.map((value, index) => (
                        <motion.div
                            key={index}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.1 }}
                            className="bg-primary rounded-xl p-6 shadow-md"
                        >
                            <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center mb-4">
                                <value.icon className="text-indigo-600" size={24} />
                            </div>
                            <h3 className="font-bold mb-2">{value.title}</h3>
                            <p className="text-sm text-secondary">{value.desc}</p>
                        </motion.div>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default AboutPage;
