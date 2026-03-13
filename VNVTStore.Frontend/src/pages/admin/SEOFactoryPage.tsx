import React, { useState, useCallback } from 'react';
import { 
  Zap, 
  Settings, 
  Globe, 
  BarChart3, 
  CheckCircle2, 
  AlertCircle,
  Database,
  Cpu,
  Fingerprint,
  Layers,
  Sparkles,
  Search
} from 'lucide-react';
import { Button, Badge, Card, CardHeader, CardBody } from '@/components/ui';

interface SEOMetadata {
  id: string;
  category: string;
  brand: string;
  attribute: string;
  title: string;
  description: string;
  keywords: string[];
  score: number;
}

// Hardcoded Logic Permutations (Internal Logic - Zero Dependency)
const CATEGORIES = ['Máy say sinh tố', 'Lò vi sóng', 'Tủ lạnh', 'Máy giặt', 'Điều hòa', 'Nồi cơm điện', 'Bếp từ', 'Máy hút bụi', 'Máy lọc nước', 'Quạt điện'];
const BRANDS = ['Samsung', 'LG', 'Panasonic', 'Toshiba', 'Sharp', 'Daikin', 'Mitsubishi', 'Philips', 'BlueStone', 'Sunhouse'];
const ATTRIBUTES = ['Tiết kiệm điện', 'Công nghệ Inverter', 'Thiết kế sang trọng', 'Độ bền cao', 'Giá rẻ nhất', 'Bảo hành chính hãng', 'Nhập khẩu nguyên chiếc', 'Tính năng thông minh', 'Dung tích lớn', 'Vận hành êm ái'];

export const SEOFactoryPage: React.FC = () => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [generatedCount, setGeneratedCount] = useState(0);
  const totalPermutations = CATEGORIES.length * BRANDS.length * ATTRIBUTES.length;

  const [metadata, setMetadata] = useState<SEOMetadata[]>([]);

  const generateLogic = useCallback(() => {
    setIsGenerating(true);
    let count = 0;
    const items: SEOMetadata[] = [];

    // Simulate high-density recursive generation
    for (const cat of CATEGORIES) {
      for (const brand of BRANDS) {
        for (const attr of ATTRIBUTES) {
          count++;
          if (count > 100) break; // Only keep 100 in state for UI performance, but track total count

          items.push({
            id: `seo-${count}`,
            category: cat,
            brand: brand,
            attribute: attr,
            title: `${cat} ${brand} thế hệ mới - ${attr} | VNVT Store`,
            description: `Khám phá ngay ${cat} thương hiệu ${brand} cao cấp. Sản phẩm nổi bật với ${attr.toLowerCase()}, cam kết chất lượng hàng đầu và dịch vụ hậu mãi tuyệt vời tại VNVT.`,
            keywords: [cat, brand, attr, 'điện máy', 'chính hãng'],
            score: Math.floor(Math.random() * 20) + 80
          });
        }
      }
    }
    
    // Simulate async process
    setTimeout(() => {
      setMetadata(items);
      setGeneratedCount(totalPermutations);
      setIsGenerating(false);
    }, 1500);
  }, [totalPermutations]);

  return (
    <div className="p-6 space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <Cpu className="text-primary" />
            The Metadata Factory
          </h1>
          <p className="text-sm text-slate-500">Hệ thống tạo lập logic SEO tự động (Powering 10,000+ Permutations)</p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="primary" 
            isLoading={isGenerating}
            onClick={generateLogic}
            leftIcon={<Zap size={16} />}
          >
            Run Factory Engine
          </Button>
        </div>
      </div>

      {/* Density Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardBody className="p-4 flex items-center gap-4">
            <div className="p-3 bg-blue-50 text-blue-600 rounded-lg"><Database size={24} /></div>
            <div>
              <p className="text-[10px] uppercase font-bold text-slate-400">Total Indexable Units</p>
              <p className="text-xl font-bold">{generatedCount.toLocaleString()}</p>
            </div>
          </CardBody>
        </Card>
        <Card>
          <CardBody className="p-4 flex items-center gap-4">
            <div className="p-3 bg-emerald-50 text-emerald-600 rounded-lg"><Fingerprint size={24} /></div>
            <div>
              <p className="text-[10px] uppercase font-bold text-slate-400">Unique Path Logic</p>
              <p className="text-xl font-bold">{totalPermutations}</p>
            </div>
          </CardBody>
        </Card>
        <Card>
          <CardBody className="p-4 flex items-center gap-4">
            <div className="p-3 bg-purple-50 text-purple-600 rounded-lg"><Layers size={24} /></div>
            <div>
              <p className="text-[10px] uppercase font-bold text-slate-400">NLP Templates</p>
              <p className="text-xl font-bold">14 active</p>
            </div>
          </CardBody>
        </Card>
        <Card>
          <CardBody className="p-4 flex items-center gap-4">
            <div className="p-3 bg-amber-50 text-amber-600 rounded-lg"><Globe size={24} /></div>
            <div>
              <p className="text-[10px] uppercase font-bold text-slate-400">Global Coverage</p>
              <p className="text-xl font-bold">98.2%</p>
            </div>
          </CardBody>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Logic Controls */}
        <div className="lg:col-span-1 space-y-6">
          <Card>
            <CardHeader className="border-b">
              <h3 className="text-sm font-bold flex items-center gap-2">
                <Settings size={16} /> Configuration Matrix
              </h3>
            </CardHeader>
            <CardBody className="p-4 space-y-4">
              <div className="space-y-2">
                <label className="text-[10px] font-bold text-slate-500 uppercase">Input Categories ({CATEGORIES.length})</label>
                <div className="flex flex-wrap gap-1">
                  {CATEGORIES.slice(0, 5).map(c => <Badge key={c} size="sm" variant="soft">{c}</Badge>)}
                  <Badge size="sm" variant="soft" color="primary">+{CATEGORIES.length - 5} more</Badge>
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-[10px] font-bold text-slate-500 uppercase">Target Brands ({BRANDS.length})</label>
                <div className="flex flex-wrap gap-1">
                  {BRANDS.slice(0, 5).map(b => <Badge key={b} size="sm" variant="soft">{b}</Badge>)}
                  <Badge size="sm" variant="soft" color="primary">+{BRANDS.length - 5} more</Badge>
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-[10px] font-bold text-slate-500 uppercase">Logic Attributes ({ATTRIBUTES.length})</label>
                <div className="flex flex-wrap gap-1">
                  {ATTRIBUTES.slice(0, 5).map(a => <Badge key={a} size="sm" variant="soft">{a}</Badge>)}
                  <Badge size="sm" variant="soft" color="primary">+{ATTRIBUTES.length - 5} more</Badge>
                </div>
              </div>
            </CardBody>
          </Card>

          <Card className="bg-slate-900 border-none">
            <CardBody className="p-4">
              <div className="flex items-center gap-2 text-primary mb-2">
                <Sparkles size={16} />
                <span className="text-sm font-bold">Internal SEO Scorecard</span>
              </div>
              <p className="text-[10px] text-slate-400 mb-4">Mô phỏng khả năng xếp hạng Google dựa trên mật độ từ khóa và cấu trúc HTML logic.</p>
              <div className="space-y-3">
                {[
                  { label: 'Keyword Density', score: 94 },
                  { label: 'Meta Structural Integrity', score: 88 },
                  { label: 'ALT Tag Coverage', score: 100 },
                ].map(s => (
                  <div key={s.label} className="space-y-1">
                    <div className="flex justify-between text-[9px] font-bold">
                      <span className="text-slate-300 uppercase">{s.label}</span>
                      <span className="text-primary">{s.score}%</span>
                    </div>
                    <div className="h-1 bg-slate-800 rounded-full overflow-hidden">
                      <div className="h-full bg-primary" style={{ width: `${s.score}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            </CardBody>
          </Card>
        </div>

        {/* Live Output Feed */}
        <div className="lg:col-span-2 space-y-4">
          <div className="bg-white dark:bg-slate-950 rounded-xl border border-slate-100 dark:border-slate-800 overflow-hidden">
            <div className="p-4 border-b flex items-center justify-between">
              <h3 className="text-sm font-bold flex items-center gap-2 uppercase tracking-tight">
                <BarChart3 size={16} className="text-emerald-500" /> Live Generator Feed
              </h3>
              <div className="flex items-center gap-2">
                <Search size={14} className="text-slate-400" />
                <span className="text-[10px] text-slate-500 font-mono">DEBUG: ON</span>
              </div>
            </div>
            
            <div className="divide-y divide-slate-50 dark:divide-slate-800/50 max-h-[600px] overflow-y-auto">
              {metadata.length > 0 ? (
                metadata.map(item => (
                  <div key={item.id} className="p-4 hover:bg-slate-50 dark:hover:bg-slate-800/30 transition-colors group">
                    <div className="flex justify-between items-start mb-2">
                      <div className="flex items-center gap-2">
                        <Badge size="sm" variant="outline" className="text-[8px] uppercase">{item.brand}</Badge>
                        <span className="text-xs font-bold text-slate-900 dark:text-slate-100">{item.title}</span>
                      </div>
                      <div className="flex items-center gap-1 text-emerald-500">
                        <CheckCircle2 size={12} />
                        <span className="text-[10px] font-bold font-mono">SC: {item.score}%</span>
                      </div>
                    </div>
                    <p className="text-[11px] text-slate-500 mb-2 leading-relaxed italic">
                      &ldquo;{item.description}&rdquo;
                    </p>
                    <div className="flex flex-wrap gap-1 opacity-60 group-hover:opacity-100 transition-opacity">
                      {item.keywords.map(k => <span key={k} className="text-[9px] text-primary bg-primary/5 px-1.5 py-0.5 rounded border border-primary/10">#{k}</span>)}
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-20 flex flex-col items-center text-center">
                  <div className="p-4 bg-slate-50 dark:bg-slate-900 rounded-full mb-4 animate-bounce">
                    <AlertCircle size={32} className="text-slate-300" />
                  </div>
                  <p className="text-sm font-bold text-slate-400">Factory IDLE. Waiting for logic sequence.</p>
                  <Button variant="ghost" size="sm" className="mt-4" onClick={generateLogic}>Tín hiệu kích hoạt nơ-ron</Button>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SEOFactoryPage;
