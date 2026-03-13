import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { MessageCircle, Send, User, ThumbsUp, AlertCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast, useAuthStore } from '@/store';
import { motion, AnimatePresence } from 'framer-motion';
import { productQAService } from '@/services/productQAService';
import { ProductQA as QAItemType } from '@/types';

interface ProductQAProps {
    productCode: string;
}

export const ProductQA: React.FC<ProductQAProps> = ({ productCode }) => {
    const { t } = useTranslation();
    const { error, success } = useToast();
    const { isAuthenticated } = useAuthStore();
    const [questions, setQuestions] = useState<QAItemType[]>([]);
    const [answers, setAnswers] = useState<QAItemType[]>([]);
    const [newQuestion, setNewQuestion] = useState("");
    const [isAsking, setIsAsking] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const fetchQuestions = useCallback(async () => {
        if (!productCode) return;
        try {
            const data = await productQAService.getByProduct(productCode);
            if (data && Array.isArray(data)) {
                const mainQuestions = data.filter(q => !q.parentCode);
                const answerItems = data.filter(q => q.parentCode);
                setQuestions(mainQuestions);
                setAnswers(answerItems);
            }
        } catch (err) {
            console.error("Failed to fetch questions", err);
        }
    }, [productCode]);

    useEffect(() => {
        fetchQuestions();
    }, [fetchQuestions]);

    const findAnswer = (questionCode: string) => {
        const ans = answers.find(a => a.parentCode === questionCode);
        if (ans) return ans;
        const q = questions.find(x => x.code === questionCode);
        if (q && q.replies && q.replies.length > 0) return q.replies[0];
        return null;
    };

    const handleAskClick = () => {
        if (!isAuthenticated) {
            error(t('auth.loginRequired', 'Vui lòng đăng nhập để thực hiện chức năng này'));
            return;
        }
        setIsAsking(!isAsking);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newQuestion.trim()) return;

        if (!isAuthenticated) {
            error(t('auth.loginRequired', 'Vui lòng đăng nhập để thực hiện chức năng này'));
            return;
        }

        setIsLoading(true);
        try {
            const result = await productQAService.create(productCode, newQuestion);
            if (result) {
                success(t('product.qa.submitSuccess'));
                setNewQuestion("");
                setIsAsking(false);
                fetchQuestions(); 
            }
        } catch (err) {
            console.error(err);
            error(t('product.qa.submitError'));
        } finally {
            setIsLoading(false);
        }
    };

    if (!productCode) return null;

    return (
        <div className="space-y-8">
            {/* Header / Ask Box */}
            <div className="bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-900/20 dark:to-purple-900/20 p-6 rounded-2xl border border-indigo-100 dark:border-indigo-900/30">
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-2">
                    <div>
                        <h3 className="text-lg font-bold flex items-center gap-2 text-slate-800 dark:text-slate-200">
                            <MessageCircle className="text-indigo-600 dark:text-indigo-400" size={24} />
                            {t('product.qa.title', 'Hỏi đáp về sản phẩm')}
                        </h3>
                        <p className="text-sm text-slate-500 dark:text-slate-400 mt-1 ml-8">
                            {t('product.qa.subtitle', 'Bạn có thắc mắc? Hãy đặt câu hỏi cho chúng tôi.')}
                        </p>
                    </div>
                    <Button onClick={handleAskClick} variant="outline" className="border-indigo-500 text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 gap-2">
                        <MessageCircle size={18} />
                        {isAsking ? t('common.cancel') : t('product.qa.askButton', 'Đặt câu hỏi')}
                    </Button>
                </div>

                <AnimatePresence>
                    {isAsking && (
                        <motion.form 
                            initial={{ height: 0, opacity: 0, marginTop: 0 }}
                            animate={{ height: 'auto', opacity: 1, marginTop: 24 }}
                            exit={{ height: 0, opacity: 0, marginTop: 0 }}
                            className="overflow-hidden"
                            onSubmit={handleSubmit}
                        >
                            <div className="flex gap-3 items-start">
                                <div className="flex-1 bg-white dark:bg-slate-800 rounded-xl p-1 border border-indigo-100 dark:border-indigo-900/50 shadow-sm focus-within:ring-2 focus-within:ring-indigo-500/20 transition-all">
                                    <Input 
                                        value={newQuestion}
                                        onChange={(e) => setNewQuestion(e.target.value)}
                                        placeholder={t('product.qa.placeholder', 'Nhập câu hỏi của bạn tối thiểu 10 ký tự...')}
                                        className="border-none bg-transparent focus:ring-0 px-4 py-3 min-h-[50px]"
                                        autoFocus
                                        disabled={isLoading}
                                    />
                                </div>
                                <Button type="submit" className="h-[50px] px-6 bg-indigo-600 hover:bg-indigo-700 text-white shadow-lg shadow-indigo-600/20" isLoading={isLoading}>
                                    <Send size={18} />
                                </Button>
                            </div>
                            <p className="text-xs text-slate-500 mt-3 pl-1 flex items-center gap-1.5">
                                <AlertCircle size={12} />
                                {t('product.qa.note', 'Câu hỏi của bạn sẽ được hiển thị sau khi được kiểm duyệt.')}
                            </p>
                        </motion.form>
                    )}
                </AnimatePresence>
            </div>

            {/* Questions List */}
            <div className="space-y-6">
                {(questions.length === 0) ? (
                    <div className="text-center py-8 text-slate-400">
                        {t('product.qa.empty', 'Chưa có câu hỏi nào. Hãy là người đầu tiên đặt câu hỏi!')}
                    </div>
                ) : (
                    questions.map((item) => {
                        const answer = findAnswer(item.code);
                        return (
                            <motion.div 
                                key={item.code}
                                initial={{ opacity: 0 }}
                                whileInView={{ opacity: 1 }}
                                className="group"
                            >
                                {/* Question */}
                                <div className="flex gap-4">
                                    <div className="w-10 h-10 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center flex-shrink-0 text-slate-500">
                                        <User size={20} />
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2 mb-1">
                                            <span className="font-bold text-sm text-slate-900 dark:text-white">{item.userName || item.userCode}</span>
                                            <span className="text-xs text-slate-400 px-2 py-0.5 bg-slate-100 dark:bg-slate-800 rounded-full">
                                                {new Date(item.createdAt).toLocaleDateString('vi-VN')}
                                            </span>
                                        </div>
                                        <p className="text-slate-700 dark:text-slate-300 font-medium">{item.comment}</p>
                                        
                                        <div className="flex items-center gap-4 mt-2 text-xs text-slate-400">
                                            <button className="flex items-center gap-1 hover:text-indigo-600 transition-colors">
                                                <ThumbsUp size={12} /> {t('common.like')} ({item.likes || 0})
                                            </button>
                                        </div>
                                    </div>
                                </div>

                                {/* Answer */}
                                {answer && (
                                    <div className="flex gap-4 mt-4 pl-14 relative">
                                        <div className="absolute left-7 top-0 bottom-0 w-0.5 bg-indigo-100 dark:bg-indigo-900/30" />
                                        <div className="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-900/50 flex items-center justify-center flex-shrink-0 text-indigo-600 dark:text-indigo-400">
                                            <MessageCircle size={14} />
                                        </div>
                                        <div className="flex-1 bg-indigo-50/50 dark:bg-indigo-900/10 p-4 rounded-xl rounded-tl-none">
                                            <div className="flex items-center gap-2 mb-1">
                                                <span className="font-bold text-sm text-indigo-700 dark:text-indigo-300">{answer.userName || 'Admin'}</span>
                                                <span className="text-[10px] items-center text-white bg-indigo-600 px-1.5 py-0.5 rounded shadow-sm">Admin</span>
                                            </div>
                                            <p className="text-slate-600 dark:text-slate-400 text-sm">{answer.comment}</p>
                                        </div>
                                    </div>
                                )}
                            </motion.div>
                        );
                    })
                )}
            </div>
        </div>
    );
};
