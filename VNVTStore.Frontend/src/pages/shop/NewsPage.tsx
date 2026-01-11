import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Calendar, ArrowRight, Clock } from 'lucide-react';
import { Link } from 'react-router-dom';

const newsItems = [
    {
        id: 1,
        title: 'VNVT Store khai trương chi nhánh mới tại Quận 7',
        excerpt: 'Nhân dịp khai trương, VNVT Store dành tặng khách hàng nhiều ưu đãi hấp dẫn...',
        date: '08/01/2026',
        category: 'Tin tức',
        image: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=400',
    },
    {
        id: 2,
        title: 'Top 10 thiết bị gia dụng bán chạy nhất 2025',
        excerpt: 'Điểm qua những sản phẩm được yêu thích nhất trong năm qua tại VNVT Store...',
        date: '05/01/2026',
        category: 'Bài viết',
        image: 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400',
    },
    {
        id: 3,
        title: 'Hướng dẫn bảo trì thiết bị gia dụng đúng cách',
        excerpt: 'Những mẹo đơn giản giúp thiết bị của bạn hoạt động bền bỉ theo thời gian...',
        date: '02/01/2026',
        category: 'Hướng dẫn',
        image: 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400',
    },
    {
        id: 4,
        title: 'Chương trình khách hàng thân thiết 2026',
        excerpt: 'Tích điểm đổi quà, nhận ưu đãi độc quyền dành cho thành viên VIP...',
        date: '30/12/2025',
        category: 'Tin tức',
        image: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=400',
    },
];

export const NewsPage = () => {
    const { t } = useTranslation();

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">📰 Tin tức & Bài viết</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        Cập nhật những thông tin mới nhất từ VNVT Store
                    </p>
                </motion.div>

                {/* News Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    {newsItems.map((news, index) => (
                        <motion.article
                            key={news.id}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.1 }}
                            className="bg-primary rounded-2xl overflow-hidden shadow-lg group"
                        >
                            <div className="relative h-48 overflow-hidden">
                                <img
                                    src={news.image}
                                    alt={news.title}
                                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                                />
                                <div className="absolute top-4 left-4">
                                    <span className="bg-primary/90 backdrop-blur px-3 py-1 rounded-full text-xs font-medium">
                                        {news.category}
                                    </span>
                                </div>
                            </div>
                            <div className="p-6">
                                <div className="flex items-center gap-2 text-sm text-tertiary mb-3">
                                    <Calendar size={14} />
                                    <span>{news.date}</span>
                                </div>
                                <h3 className="text-lg font-bold mb-2 line-clamp-2 group-hover:text-primary transition-colors">
                                    {news.title}
                                </h3>
                                <p className="text-secondary text-sm mb-4 line-clamp-2">
                                    {news.excerpt}
                                </p>
                                <Link
                                    to={`/news/${news.id}`}
                                    className="inline-flex items-center gap-2 text-primary text-sm font-medium hover:gap-3 transition-all"
                                >
                                    Đọc thêm <ArrowRight size={16} />
                                </Link>
                            </div>
                        </motion.article>
                    ))}
                </div>

                {/* Load More */}
                <div className="text-center mt-12">
                    <button className="px-8 py-3 border-2 border-primary text-primary rounded-full font-medium hover:bg-primary hover:text-white transition-colors">
                        Xem thêm bài viết
                    </button>
                </div>
            </div>
        </div>
    );
};

export default NewsPage;
