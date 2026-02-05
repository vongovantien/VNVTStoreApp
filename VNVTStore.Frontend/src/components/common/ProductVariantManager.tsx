import { useTranslation } from 'react-i18next';
import { useState, useEffect } from 'react';
import { Button, Input, Badge } from '@/components/ui';
import { RefreshCw, X, Plus, Trash2 } from 'lucide-react';

export interface ProductVariantData {
  sku: string;
  attributes: Record<string, string>;
  price: number;
  stockQuantity: number;
}

interface ProductVariantManagerProps {
  initialVariants?: ProductVariantData[];
  basePrice: number;
  productCode?: string;
  onChange: (variants: ProductVariantData[]) => void;
}

interface Attribute {
  name: string;
  values: string[];
}

export const ProductVariantManager = ({
  initialVariants = [],
  basePrice,
  productCode,
  onChange
}: ProductVariantManagerProps) => {
  const { t } = useTranslation();
  const [attributes, setAttributes] = useState<Attribute[]>([]);
  const [variants, setVariants] = useState<ProductVariantData[]>(initialVariants);

  // Parse initial attributes from variants if any
  useEffect(() => {
    if (initialVariants.length > 0 && attributes.length === 0) {
      const attrMap: Record<string, Set<string>> = {};
      initialVariants.forEach(v => {
        const attrs = typeof v.attributes === 'string' ? JSON.parse(v.attributes) : v.attributes;
        Object.entries(attrs).forEach(([name, value]) => {
          if (!attrMap[name]) attrMap[name] = new Set<string>();
          attrMap[name].add(value as string);
        });
      });
      // eslint-disable-next-line
      setAttributes(Object.entries(attrMap).map(([name, values]) => ({
        name,
        values: Array.from(values)
      })));
    }
  }, [initialVariants, attributes.length]);

  const addAttribute = () => setAttributes([...attributes, { name: '', values: [] }]);
  
  const updateAttributeName = (idx: number, name: string) => {
    const newAttrs = [...attributes];
    newAttrs[idx].name = name;
    setAttributes(newAttrs);
  };

  const addAttributeValue = (idx: number, value: string) => {
    if (!value.trim()) return;
    const newAttrs = [...attributes];
    if (!newAttrs[idx].values.includes(value)) {
      newAttrs[idx].values.push(value);
      setAttributes(newAttrs);
    }
  };

  const removeAttributeValue = (attrIdx: number, valIdx: number) => {
    const newAttrs = [...attributes];
    newAttrs[attrIdx].values.splice(valIdx, 1);
    setAttributes(newAttrs);
  };

  const removeAttribute = (idx: number) => {
    setAttributes(attributes.filter((_, i) => i !== idx));
  };

  const generateVariants = () => {
    const validAttrs = attributes.filter(a => a.name.trim() && a.values.length > 0);
    if (validAttrs.length === 0) return;

    const combinations = validAttrs.reduce((acc, attr) => {
      const next: Record<string, string>[] = [];
      acc.forEach(prevCombo => {
        attr.values.forEach(val => {
          next.push({ ...prevCombo, [attr.name]: val });
        });
      });
      return next;
    }, [{} as Record<string, string>]);

    const newVariants = combinations.map(combo => {
      const attrStr = Object.values(combo).join('-');
      // Check if variant with these attributes already exists to preserve its data
      const existing = variants.find(v => {
          const vAttrs = typeof v.attributes === 'string' ? JSON.parse(v.attributes) : v.attributes;
          return JSON.stringify(vAttrs) === JSON.stringify(combo);
      });
      
      return existing || {
        sku: `${productCode || 'PROD'}-${attrStr.toUpperCase()}`,
        attributes: combo,
        price: basePrice,
        stockQuantity: 0
      };
    });

    setVariants(newVariants);
    onChange(newVariants);
  };

  const updateVariant = (idx: number, field: keyof ProductVariantData, value: string | number | Record<string, string>) => {
    const newVariants = [...variants];
    newVariants[idx] = { ...newVariants[idx], [field]: value };
    setVariants(newVariants);
    onChange(newVariants);
  };

  return (
    <div className="space-y-6">
      <div className="space-y-4 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color">
        <div className="flex justify-between items-center mb-2">
            <h4 className="text-sm font-semibold text-primary">{t('product.variantAttributes')}</h4>
            <Button type="button" size="sm" variant="outline" onClick={addAttribute}>
                <Plus size={14} className="mr-1" /> {t('common.add')}
            </Button>
        </div>
        
        {attributes.length === 0 ? (
            <p className="text-center text-sm text-tertiary py-4 italic">{t('product.noAttributes')}</p>
        ) : (
            attributes.map((attr, idx) => (
                <div key={idx} className="space-y-2 pb-4 border-b border-border-color last:border-0 last:pb-0">
                    <div className="flex gap-2 items-center">
                        <Input 
                            className="flex-1 h-9 text-sm font-medium" 
                            placeholder={t('common.fields.attributeName')}
                            value={attr.name}
                            onChange={(e) => updateAttributeName(idx, e.target.value)}
                        />
                        <button type="button" onClick={() => removeAttribute(idx)} className="text-slate-400 hover:text-red-500">
                            <Trash2 size={16} />
                        </button>
                    </div>
                    <div className="flex flex-wrap gap-2 items-center">
                        {attr.values.map((val, vIdx) => (
                            <Badge key={vIdx} color="primary" className="flex items-center gap-1 pr-1 py-1">
                                {val}
                                <button type="button" onClick={() => removeAttributeValue(idx, vIdx)} className="hover:bg-slate-200/20 rounded-full p-0.5">
                                    <X size={12} />
                                </button>
                            </Badge>
                        ))}
                        <Input 
                            className="w-32 h-7 text-xs" 
                            placeholder={t('common.addValue')}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter') {
                                    e.preventDefault();
                                    addAttributeValue(idx, e.currentTarget.value);
                                    e.currentTarget.value = '';
                                }
                            }}
                        />
                    </div>
                </div>
            ))
        )}

        {attributes.length > 0 && (
            <div className="pt-2 flex justify-center">
                <Button type="button" size="sm" onClick={generateVariants} className="gap-2">
                    <RefreshCw size={14} /> {t('product.generateVariants')}
                </Button>
            </div>
        )}
      </div>

      {variants.length > 0 && (
        <div className="space-y-3">
            <h4 className="text-sm font-semibold text-primary px-1">{t('product.variants')}</h4>
            <div className="bg-white dark:bg-slate-950 rounded-xl border border-border-color overflow-hidden scrollbar-thin overflow-x-auto">
                <table className="w-full text-sm min-w-[600px]">
                    <thead>
                        <tr className="bg-slate-50 dark:bg-slate-900/50 text-secondary text-xs font-semibold border-b border-border-color">
                            <th className="px-4 py-3 text-left">{t('product.variant')}</th>
                            <th className="px-4 py-3 text-left">SKU</th>
                            <th className="px-4 py-3 text-left">{t('common.fields.price')}</th>
                            <th className="px-4 py-3 text-left">{t('common.fields.stock')}</th>
                            <th className="px-4 py-3 text-center"></th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-border-color">
                        {variants.map((variant, idx) => (
                            <tr key={idx} className="hover:bg-slate-50/50 dark:hover:bg-slate-900/20 transition-colors">
                                <td className="px-4 py-3">
                                    <div className="flex flex-wrap gap-1">
                                        {Object.entries(typeof variant.attributes === 'string' ? JSON.parse(variant.attributes) : variant.attributes).map(([k, v]) => (
                                            <Badge key={k} variant="outline" className="text-[10px] lowercase py-0">{k}: {v as string}</Badge>
                                        ))}
                                    </div>
                                </td>
                                <td className="px-4 py-3">
                                    <Input 
                                        className="h-8 text-xs w-full" 
                                        value={variant.sku} 
                                        onChange={(e) => updateVariant(idx, 'sku', e.target.value)}
                                    />
                                </td>
                                <td className="px-4 py-3">
                                    <Input 
                                        type="number"
                                        className="h-8 text-xs w-32" 
                                        value={variant.price} 
                                        onChange={(e) => updateVariant(idx, 'price', Number(e.target.value))}
                                    />
                                </td>
                                <td className="px-4 py-3">
                                    <Input 
                                        type="number"
                                        className="h-8 text-xs w-24" 
                                        value={variant.stockQuantity} 
                                        onChange={(e) => updateVariant(idx, 'stockQuantity', Number(e.target.value))}
                                    />
                                </td>
                                <td className="px-4 py-3 text-center">
                                    <button type="button" onClick={() => {
                                        const newVariants = variants.filter((_, i) => i !== idx);
                                        setVariants(newVariants);
                                        onChange(newVariants);
                                    }} className="text-slate-400 hover:text-red-500">
                                        <Trash2 size={16} />
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
      )}
    </div>
  );
};
