import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { MapPin, Phone, Mail, Clock, Send } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast } from '@/store';

export const ContactPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        phone: '',
        subject: '',
        message: '',
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        toast.success('Cảm ơn bạn! Chúng tôi sẽ liên hệ lại trong thời gian sớm nhất.');
        setFormData({ name: '', email: '', phone: '', subject: '', message: '' });
    };

    const contactInfo = [
        { icon: MapPin, title: 'Địa chỉ', content: '123 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh' },
        { icon: Phone, title: 'Hotline', content: '1900 123 456' },
        { icon: Mail, title: 'Email', content: 'contact@vnvtstore.com' },
        { icon: Clock, title: 'Giờ làm việc', content: '8:00 - 21:00 (Thứ 2 - CN)' },
    ];

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">📞 Liên hệ với chúng tôi</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        Chúng tôi luôn sẵn sàng hỗ trợ bạn. Hãy liên hệ ngay!
                    </p>
                </motion.div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
                    {/* Contact Form */}
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        className="bg-primary rounded-2xl p-8 shadow-lg"
                    >
                        <h2 className="text-2xl font-bold mb-6">Gửi tin nhắn</h2>
                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <Input
                                    label="Họ tên *"
                                    placeholder="Nguyễn Văn A"
                                    value={formData.name}
                                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                    required
                                />
                                <Input
                                    label="Email *"
                                    type="email"
                                    placeholder="email@example.com"
                                    value={formData.email}
                                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                    required
                                />
                            </div>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <Input
                                    label="Số điện thoại"
                                    placeholder="0901 234 567"
                                    value={formData.phone}
                                    onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                                />
                                <Input
                                    label="Chủ đề"
                                    placeholder="Hỗ trợ sản phẩm"
                                    value={formData.subject}
                                    onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-2">Nội dung *</label>
                                <textarea
                                    rows={5}
                                    placeholder="Nhập nội dung tin nhắn..."
                                    value={formData.message}
                                    onChange={(e) => setFormData({ ...formData, message: e.target.value })}
                                    className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:border-primary resize-none"
                                    required
                                />
                            </div>
                            <Button type="submit" fullWidth rightIcon={<Send size={18} />}>
                                Gửi tin nhắn
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
                        <div className="bg-primary rounded-2xl overflow-hidden shadow-lg">
                            <div className="h-64 bg-gradient-to-br from-indigo-100 to-purple-100 flex items-center justify-center">
                                <div className="text-center">
                                    <MapPin size={48} className="mx-auto mb-2 text-indigo-600" />
                                    <p className="text-indigo-800 font-medium">Bản đồ</p>
                                    <p className="text-indigo-600 text-sm">123 Nguyễn Huệ, Q.1, TP.HCM</p>
                                </div>
                            </div>
                        </div>

                        {/* Social Links */}
                        <div className="bg-primary rounded-xl p-6 shadow-md">
                            <h3 className="font-semibold mb-4">Kết nối với chúng tôi</h3>
                            <div className="flex gap-4">
                                {['Facebook', 'Zalo', 'Instagram', 'Youtube'].map((social) => (
                                    <button
                                        key={social}
                                        className="px-4 py-2 bg-secondary rounded-lg text-sm font-medium hover:bg-primary/10 transition-colors"
                                    >
                                        {social}
                                    </button>
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
