import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Calendar, ArrowLeft } from 'lucide-react';

const newsItems = [
    {
        id: 1,
        title: 'VNVT Store khai trương chi nhánh mới tại Quận 7',
        content: 'Nội dung chi tiết về việc khai trương chi nhánh mới...',
        date: '08/01/2026',
        category: 'Tin tức',
        image: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800',
    },
    {
        id: 2,
        title: 'Top 10 thiết bị gia dụng bán chạy nhất 2025',
        content: 'Nội dung chi tiết về top 10 sản phẩm bán chạy...',
        date: '05/01/2026',
        category: 'Bài viết',
        image: 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=800',
    },
    {
        id: 3,
        title: 'Hướng dẫn bảo trì thiết bị gia dụng đúng cách',
        content: 'Nội dung hướng dẫn chi tiết...',
        date: '02/01/2026',
        category: 'Hướng dẫn',
        image: 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800',
    },
    {
        id: 4,
        title: 'Chương trình khách hàng thân thiết 2026',
        content: 'Nội dung chi tiết về chương trình khách hàng thân thiết...',
        date: '30/12/2025',
        category: 'Tin tức',
        image: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=800',
    },
];

const NewsDetailPage = () => {
    const { id } = useParams();
    const item = newsItems.find(n => n.id === Number(id));

    if (!item) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl font-bold mb-4">Không tìm thấy bài viết</h2>
                    <Link to="/news" className="text-primary hover:underline">Quay lại tin tức</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4 max-w-4xl">
                <Link to="/news" className="inline-flex items-center gap-2 text-primary mb-8 hover:gap-3 transition-all">
                    <ArrowLeft size={20} /> Quay lại tin tức
                </Link>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="bg-primary rounded-3xl overflow-hidden shadow-xl"
                >
                    <img src={item.image} alt={item.title} className="w-full h-[400px] object-cover" />
                    
                    <div className="p-8 md:p-12">
                        <div className="flex items-center gap-4 text-sm text-tertiary mb-6">
                            <span className="bg-secondary px-3 py-1 rounded-full font-medium text-primary">
                                {item.category}
                            </span>
                            <div className="flex items-center gap-1">
                                <Calendar size={14} />
                                <span>{item.date}</span>
                            </div>
                        </div>

                        <h1 className="text-3xl md:text-4xl font-extrabold mb-8">{item.title}</h1>
                        
                        <div className="prose prose-lg dark:prose-invert max-w-none">
                            <p className="text-secondary leading-relaxed whitespace-pre-line">
                                {item.content}
                            </p>
                            <p className="mt-8 text-secondary">
                                Đây là bài viết mẫu. Nội dung thực tế sẽ được cập nhật từ hệ thống quản trị tin tức.
                            </p>
                        </div>
                    </div>
                </motion.div>
            </div>
        </div>
    );
};

export default NewsDetailPage;
