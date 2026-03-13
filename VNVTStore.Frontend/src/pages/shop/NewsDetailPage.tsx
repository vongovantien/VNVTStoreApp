import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Calendar, ArrowLeft, User } from 'lucide-react';
import SharedImage from '@/components/common/Image';
import { newsService, type NewsDto } from '@/services/newsService';
import { useSEO } from '@/hooks/useSEO';

const NewsDetailPage = () => {
    const { t } = useTranslation();
    const { id } = useParams();

    const { data: newsResponse, isLoading } = useQuery({
        queryKey: ['news', 'detail', id],
        queryFn: () => newsService.getByCode(id!),
        enabled: !!id,
    });

    const item: NewsDto | null = newsResponse?.data ?? null;

    useSEO({
        title: item?.title || t('news.title', 'Tin tức'),
        description: item?.summary || item?.content?.substring(0, 160) || 'Chi tiết bài viết tại VNVT Store',
        ogImage: item?.thumbnail,
        ogType: 'article',
        canonicalPath: item ? `/news/${id}` : undefined,
    });

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
        });
    };

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
            </div>
        );
    }

    if (!item) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl font-bold mb-4">{t('news.notFound', 'Không tìm thấy bài viết')}</h2>
                    <Link to="/news" className="text-primary hover:underline">
                        {t('news.backToList', 'Quay lại tin tức')}
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4 max-w-4xl">
                <Link to="/news" className="inline-flex items-center gap-2 text-primary mb-8 hover:gap-3 transition-all">
                    <ArrowLeft size={20} /> {t('news.backToList', 'Quay lại tin tức')}
                </Link>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="bg-primary rounded-3xl overflow-hidden shadow-xl"
                >
                    {item.thumbnail && (
                        <SharedImage
                            src={item.thumbnail}
                            alt={item.title}
                            className="w-full h-[400px] object-cover"
                        />
                    )}
                    
                    <div className="p-8 md:p-12">
                        <div className="flex items-center gap-4 text-sm text-tertiary mb-6 flex-wrap">
                            {item.author && (
                                <span className="bg-secondary px-3 py-1 rounded-full font-medium text-primary flex items-center gap-1">
                                    <User size={14} /> {item.author}
                                </span>
                            )}
                            <div className="flex items-center gap-1">
                                <Calendar size={14} />
                                <span>{formatDate(item.publishedAt || item.createdAt)}</span>
                            </div>
                        </div>

                        <h1 className="text-3xl md:text-4xl font-extrabold mb-4">{item.title}</h1>
                        
                        {item.summary && (
                            <p className="text-lg text-secondary mb-8 italic">{item.summary}</p>
                        )}
                        
                        <div
                            className="prose prose-lg dark:prose-invert max-w-none"
                            dangerouslySetInnerHTML={{ __html: item.content || '' }}
                        />
                    </div>
                </motion.div>
            </div>
        </div>
    );
};

export default NewsDetailPage;
