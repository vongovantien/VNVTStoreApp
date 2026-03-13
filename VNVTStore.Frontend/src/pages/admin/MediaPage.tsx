import React, { useState, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Image as ImageIcon, 
  Search, 
  Grid, 
  List, 
  Download, 
  Trash2, 
  CheckCircle, 
  Info,
  ExternalLink,
  RefreshCw,
  HardDrive
} from 'lucide-react';
import { Button, Badge } from '@/components/ui';
import { cn } from '@/utils/cn';

interface MediaAsset {
  id: string;
  url: string;
  name: string;
  size: string;
  dimensions: string;
  type: 'product' | 'banner' | 'ui';
  createdAt: string;
}

export const MediaPage: React.FC = () => {
  const [view, setView] = useState<'grid' | 'list'>('grid');
  const [search, setSearch] = useState('');
  const [filterType, setFilterType] = useState<string>('all');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Mock data for demonstration of high density
  const assets: MediaAsset[] = useMemo(() => [
    { id: '1', url: '/images/products/p1.jpg', name: 'Premium Blender XL', size: '1.2MB', dimensions: '1200x1200px', type: 'product', createdAt: '2024-03-10' },
    { id: '2', url: '/images/products/p2.jpg', name: 'Smart Toaster v2', size: '850KB', dimensions: '1000x1000px', type: 'product', createdAt: '2024-03-11' },
    { id: '3', url: '/images/banners/sale.jpg', name: 'Summer Sale Hero', size: '3.5MB', dimensions: '1920x600px', type: 'banner', createdAt: '2024-03-05' },
    { id: '4', url: '/images/ui/logo.png', name: 'Company Logo', size: '45KB', dimensions: '512x512px', type: 'ui', createdAt: '2024-01-01' },
    // Repeat more for density feel
    ...Array.from({ length: 12 }).map((_, i) => ({
      id: `gen-${i}`,
      url: `/images/placeholder-${i % 5}.jpg`,
      name: `Asset_${i + 10}00.webp`,
      size: `${((i * 1.7) % 2.5).toFixed(1)}MB`,
      dimensions: '800x800px',
      type: (i % 3 === 0 ? 'product' : 'banner') as 'product' | 'banner' | 'ui',
      createdAt: '2024-03-12'
    }))
  ], []);

  const filteredAssets = assets.filter(a => 
    (filterType === 'all' || a.type === filterType) &&
    (a.name.toLowerCase().includes(search.toLowerCase()))
  );

  const toggleSelect = (id: string) => {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    setSelectedIds(next);
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <HardDrive className="text-primary" />
            Media Asset Manager
          </h1>
          <p className="text-sm text-slate-500">Quản lý và tối ưu hóa tài nguyên hình ảnh toàn hệ thống</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" leftIcon={<RefreshCw size={16} />}>Đồng bộ Storage</Button>
          <Button leftIcon={<ImageIcon size={16} />}>Tải lên mới</Button>
        </div>
      </div>

      {/* Stats Bar */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[
          { label: 'Tổng dung lượng', value: '45.2 GB', color: 'bg-blue-50 text-blue-700' },
          { label: 'Số lượng file', value: '1,248', color: 'bg-emerald-50 text-emerald-700' },
          { label: 'Hình ảnh lỗi', value: '0', color: 'bg-rose-50 text-rose-700' },
          { label: 'Cần tối ưu', value: '14', color: 'bg-amber-50 text-amber-700' },
        ].map((stat, i) => (
          <div key={i} className={cn("p-4 rounded-xl border border-transparent", stat.color)}>
            <p className="text-[10px] uppercase font-bold tracking-wider opacity-70">{stat.label}</p>
            <p className="text-xl font-bold">{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Toolbar */}
      <div className="bg-white dark:bg-slate-900 p-4 rounded-xl border border-slate-100 dark:border-slate-800 flex flex-wrap items-center gap-4">
        <div className="relative flex-1 min-w-[200px]">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
          <input 
            type="text" 
            placeholder="Tìm kiếm theo tên file..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 dark:bg-slate-800 border-none rounded-lg text-sm"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className="flex items-center gap-2">
          <button onClick={() => setFilterType('all')}>
            <Badge color={filterType === 'all' ? 'primary' : 'default'} variant={filterType === 'all' ? 'solid' : 'outline'} className="cursor-pointer">Tất cả</Badge>
          </button>
          <button onClick={() => setFilterType('product')}>
            <Badge color={filterType === 'product' ? 'primary' : 'default'} variant={filterType === 'product' ? 'solid' : 'outline'} className="cursor-pointer">Sản phẩm</Badge>
          </button>
          <button onClick={() => setFilterType('banner')}>
            <Badge color={filterType === 'banner' ? 'primary' : 'default'} variant={filterType === 'banner' ? 'solid' : 'outline'} className="cursor-pointer">Banner</Badge>
          </button>
        </div>

        <div className="h-6 w-px bg-slate-200 dark:bg-slate-800 mx-2" />

        <div className="flex bg-slate-50 dark:bg-slate-800 p-1 rounded-lg">
          <button 
            onClick={() => setView('grid')}
            className={cn("p-1.5 rounded", view === 'grid' ? "bg-white dark:bg-slate-700 shadow-sm text-primary" : "text-slate-500")}
          >
            <Grid size={18} />
          </button>
          <button 
            onClick={() => setView('list')}
            className={cn("p-1.5 rounded", view === 'list' ? "bg-white dark:bg-slate-700 shadow-sm text-primary" : "text-slate-500")}
          >
            <List size={18} />
          </button>
        </div>
      </div>

      {/* Bulk Actions (Conditional) */}
      <AnimatePresence>
        {selectedIds.size > 0 && (
          <motion.div 
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className="bg-primary/5 border border-primary/20 p-3 rounded-xl flex items-center justify-between"
          >
            <div className="flex items-center gap-3">
              <CheckCircle className="text-primary" size={20} />
              <span className="text-sm font-medium">Đã chọn <strong>{selectedIds.size}</strong> tài nguyên</span>
            </div>
            <div className="flex gap-2">
              <Button size="sm" variant="outline" leftIcon={<Download size={14} />}>Tải xuống</Button>
              <Button size="sm" variant="outline" leftIcon={<ImageIcon size={14} />}>Gắn thẻ</Button>
              <Button size="sm" variant="danger" leftIcon={<Trash2 size={14} />}>Xóa vĩnh viễn</Button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Content */}
      {view === 'grid' ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
          {filteredAssets.map(asset => (
            <div 
              key={asset.id}
              onClick={() => toggleSelect(asset.id)}
              className={cn(
                "group relative bg-white dark:bg-slate-900 border rounded-xl overflow-hidden cursor-pointer transition-all",
                selectedIds.has(asset.id) ? "ring-2 ring-primary border-transparent" : "hover:border-primary/50"
              )}
            >
              <div className="aspect-square bg-slate-50 dark:bg-slate-800 relative">
                <img src={asset.url} alt={asset.name} className="w-full h-full object-cover" />
                <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                   <button className="p-2 bg-white rounded-full text-slate-800 hover:scale-110 transition-transform">
                     <ImageIcon size={16} />
                   </button>
                   <button className="p-2 bg-white rounded-full text-slate-800 hover:scale-110 transition-transform">
                     <ExternalLink size={16} />
                   </button>
                </div>
                {selectedIds.has(asset.id) && (
                  <div className="absolute top-2 right-2 bg-primary text-white rounded-full p-1">
                    <CheckCircle size={12} />
                  </div>
                )}
              </div>
              <div className="p-2">
                <p className="text-[10px] font-bold truncate mb-0.5">{asset.name}</p>
                <div className="flex justify-between text-[9px] text-slate-500">
                  <span>{asset.dimensions}</span>
                  <span>{asset.size}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-xl overflow-hidden">
          <table className="w-full text-sm text-left">
            <thead className="bg-slate-50 dark:bg-slate-800/50">
              <tr>
                <th className="px-6 py-4 font-medium">Preview</th>
                <th className="px-6 py-4 font-medium">Tên file</th>
                <th className="px-6 py-4 font-medium">Thông số</th>
                <th className="px-6 py-4 font-medium">Dung lượng</th>
                <th className="px-6 py-4 font-medium text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {filteredAssets.map(asset => (
                <tr key={asset.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/30 transition-colors">
                  <td className="px-6 py-4">
                    <img src={asset.url} alt={asset.name} className="w-10 h-10 rounded border object-cover" />
                  </td>
                  <td className="px-6 py-4 font-medium">{asset.name}</td>
                  <td className="px-6 py-4 text-slate-500 font-mono text-xs">{asset.dimensions}</td>
                  <td className="px-6 py-4 text-slate-500">{asset.size}</td>
                  <td className="px-6 py-4 flex justify-end gap-2">
                    <button className="p-2 hover:text-primary transition-colors"><Info size={16} /></button>
                    <button className="p-2 hover:text-rose-500 transition-colors"><Trash2 size={16} /></button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default MediaPage;
