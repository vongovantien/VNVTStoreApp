import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Calendar, ArrowRight, Newspaper } from 'lucide-react';
import { Link } from 'react-router-dom';
import SharedImage from '@/components/common/Image';
import { newsService, type NewsDto } from '@/services/newsService';
import { useSEO } from '@/hooks/useSEO';
import { Button } from '@/components/ui';

export const NewsPage = () => {
    const { t } = useTranslation();
    const [page, setPage] = useState(1);
    const pageSize = 10;

    useSEO({
        title: 'Tin tức & Bài viết',
        description: 'Cập nhật những thông tin mới nhất, hướng dẫn sử dụng và mẹo vặt về đồ gia dụng từ VNVT Store.',
        canonicalPath: '/news',
    });

    const { data: newsResponse, isLoading } = useQuery({
        queryKey: ['news', 'shop', page],
        queryFn: () => newsService.search({
            pageIndex: page,
            pageSize,
            sortBy: 'createdAt',
            sortDesc: true,
            filters: [{ field: 'isActive', value: true }],
        }),
    });

    const newsItems: NewsDto[] = newsResponse?.data?.items || [];
    const totalPages = newsResponse?.data?.totalPages || 1;

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
        });
    };

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">📰 {t('news.title', 'Tin tức & Bài viết')}</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        {t('news.subtitle', 'Cập nhật những thông tin mới nhất từ VNVT Store')}
                    </p>
                </motion.div>

                {/* Loading */}
                {isLoading && (
                    <div className="flex justify-center py-12">
                        <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
                    </div>
                )}

                {/* Empty State */}
                {!isLoading && newsItems.length === 0 && (
                    <div className="text-center py-16">
                        <div className="w-20 h-20 bg-tertiary rounded-full flex items-center justify-center mx-auto mb-4">
                            <Newspaper size={40} className="text-tertiary" />
                        </div>
                        <h3 className="text-xl font-bold mb-2">{t('news.empty', 'Chưa có bài viết nào')}</h3>
                        <p className="text-secondary">{t('news.emptyDesc', 'Hãy quay lại sau để xem những bài viết mới nhất.')}</p>
                    </div>
                )}

                {/* News Grid */}
                {!isLoading && newsItems.length > 0 && (
                    <>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                            {newsItems.map((news, index) => (
                                <motion.article
                                    key={news.code}
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ delay: index * 0.1 }}
                                    className="bg-primary rounded-2xl overflow-hidden shadow-lg group"
                                >
                                    <div className="relative h-48 overflow-hidden">
                                        <SharedImage
                                            src={news.thumbnail || 'https://images.unsplash.com/photo-1504711434969-e33886168d6c?w=400'}
                                            alt={news.title}
                                            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                                        />
                                        {news.author && (
                                            <div className="absolute top-4 left-4">
                                                <span className="bg-primary/90 backdrop-blur px-3 py-1 rounded-full text-xs font-medium">
                                                    {news.author}
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                    <div className="p-6">
                                        <div className="flex items-center gap-2 text-sm text-tertiary mb-3">
                                            <Calendar size={14} />
                                            <span>{formatDate(news.publishedAt || news.createdAt)}</span>
                                        </div>
                                        <h3 className="text-lg font-bold mb-2 line-clamp-2 group-hover:text-primary transition-colors">
                                            {news.title}
                                        </h3>
                                        <p className="text-secondary text-sm mb-4 line-clamp-2">
                                            {news.summary || t('news.noSummary', 'Xem chi tiết bài viết...')}
                                        </p>
                                        <Link
                                            to={`/news/${news.code}`}
                                            className="inline-flex items-center gap-2 text-primary text-sm font-medium hover:gap-3 transition-all"
                                        >
                                            {t('news.readMore', 'Đọc thêm')} <ArrowRight size={16} />
                                        </Link>
                                    </div>
                                </motion.article>
                            ))}
                        </div>

                        {/* Pagination */}
                        {totalPages > 1 && (
                            <div className="flex justify-center gap-2 mt-12">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    disabled={page <= 1}
                                    onClick={() => setPage(p => p - 1)}
                                >
                                    {t('common.prev', 'Trước')}
                                </Button>
                                <span className="flex items-center px-4 text-sm text-secondary">
                                    {page} / {totalPages}
                                </span>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    disabled={page >= totalPages}
                                    onClick={() => setPage(p => p + 1)}
                                >
                                    {t('common.next', 'Sau')}
                                </Button>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};

export default NewsPage;
