import React, { useState } from 'react';
import { MessageCircle, Phone, X, MessageSquare } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useTranslation } from 'react-i18next';

import { ChatWidget } from './ChatWidget';

export const FloatingContact: React.FC = () => {
    const { t } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);
    const [isChatOpen, setIsChatOpen] = useState(false);

    const contactMethods = [
        {
            name: 'AI Assistant',
            icon: <MessageSquare size={20} />,
            color: 'bg-gradient-to-r from-primary to-purple-500',
            onClick: () => setIsChatOpen(true),
            label: t('support.widgets.aiAssistant', 'AI Assistant')
        },
        {
            name: 'Zalo',
            icon: <MessageSquare size={20} />,
            color: 'bg-blue-600',
            link: 'https://zalo.me/0901234567', // Example number
            label: t('support.widgets.chatZalo', 'Chat Zalo')
        },
        {
            name: 'WhatsApp',
            icon: <MessageCircle size={20} />,
            color: 'bg-green-500',
            link: 'https://wa.me/84901234567',
            label: t('support.widgets.whatsApp', 'WhatsApp')
        },
        {
            name: 'Call Support',
            icon: <Phone size={20} />,
            color: 'bg-indigo-600',
            link: 'tel:0901234567',
            label: t('support.widgets.callSupport', 'Gọi hỗ trợ')
        },
    ];

    return (
        <>
            <div className="fixed bottom-8 right-8 z-50 flex flex-col items-end gap-4">
                <AnimatePresence>
                    {isOpen && (
                        <motion.div
                            initial={{ opacity: 0, y: 20, scale: 0.8 }}
                            animate={{ opacity: 1, y: 0, scale: 1 }}
                            exit={{ opacity: 0, y: 20, scale: 0.8 }}
                            className="flex flex-col gap-3 mb-2"
                        >
                            {contactMethods.map((method, index) => (
                                <motion.a
                                    key={method.name}
                                    href={method.link}
                                    onClick={(e) => {
                                        if (method.onClick) {
                                            e.preventDefault();
                                            method.onClick();
                                            setIsOpen(false);
                                        }
                                    }}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    initial={{ opacity: 0, x: 20 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    transition={{ delay: index * 0.1 }}
                                    className={`flex items-center gap-3 px-4 py-3 rounded-full text-white shadow-xl hover:scale-105 transition-transform cursor-pointer ${method.color}`}
                                >
                                    <span className="text-xs font-bold uppercase tracking-wider">{method.label}</span>
                                    {method.icon}
                                </motion.a>
                            ))}
                        </motion.div>
                    )}
                </AnimatePresence>

                <button
                    onClick={() => setIsOpen(!isOpen)}
                    className={`w-14 h-14 rounded-full flex items-center justify-center text-white shadow-2xl transition-all duration-500 ${
                        isOpen 
                            ? 'bg-slate-800 rotate-90' 
                            : 'bg-gradient-to-r from-primary to-purple-500 hover:scale-110 active:scale-95'
                    }`}
                >
                    {isOpen ? <X size={28} /> : <MessageCircle size={28} />}
                    {!isOpen && (
                        <span className="absolute -top-1 -right-1 w-4 h-4 bg-rose-500 rounded-full border-2 border-white animate-ping" />
                    )}
                </button>
            </div>

            <ChatWidget 
                isOpen={isChatOpen} 
                onClose={() => setIsChatOpen(false)} 
            />
        </>
    );
};
