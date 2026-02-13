/**
 * SEO Hooks for VNVT Store
 * Feature #85: SEO Head Management (title, meta, OG, Twitter Card)
 * Feature #86: Canonical Tag Management
 * Feature #87: Schema.org JSON-LD (Product, Organization, Breadcrumb)
 * 
 * All self-contained, no third-party dependencies.
 */
import { useEffect } from 'react';

const SITE_NAME = 'VNVT Store';
const DEFAULT_OG_IMAGE = '/og-image.png';
const SITE_URL = typeof window !== 'undefined' ? window.location.origin : 'https://vnvtstore.com';

// ============ Helper: Set/Remove Meta Tags ============
const setMetaTag = (property: string, content: string, isProperty = false) => {
    const attr = isProperty ? 'property' : 'name';
    let el = document.querySelector(`meta[${attr}="${property}"]`) as HTMLMetaElement;
    if (!el) {
        el = document.createElement('meta');
        el.setAttribute(attr, property);
        document.head.appendChild(el);
    }
    el.content = content;
    return el;
};

const removeMetaTag = (property: string, isProperty = false) => {
    const attr = isProperty ? 'property' : 'name';
    const el = document.querySelector(`meta[${attr}="${property}"]`);
    if (el) el.remove();
};

// ============ #86 Canonical Tag ============
export const useCanonical = (path?: string) => {
    useEffect(() => {
        const url = path
            ? `${window.location.origin}${path}`
            : window.location.href.split('?')[0];

        let link = document.querySelector('link[rel="canonical"]') as HTMLLinkElement;
        if (!link) {
            link = document.createElement('link');
            link.rel = 'canonical';
            document.head.appendChild(link);
        }
        link.href = url;

        return () => {
            if (link.parentNode) link.parentNode.removeChild(link);
        };
    }, [path]);
};

// ============ #87 Schema.org JSON-LD ============
interface ProductSchemaProps {
    name: string;
    description?: string;
    imageUrl?: string;
    price?: number;
    currency?: string;
    brand?: string;
    sku?: string;
    rating?: number;
    reviewCount?: number;
    availability?: 'InStock' | 'OutOfStock' | 'PreOrder';
}

export const useProductSchema = (product: ProductSchemaProps | null) => {
    useEffect(() => {
        if (!product) return;

        const schema: Record<string, unknown> = {
            '@context': 'https://schema.org',
            '@type': 'Product',
            name: product.name,
            description: product.description || '',
            image: product.imageUrl || '',
            sku: product.sku || '',
            brand: product.brand ? {
                '@type': 'Brand',
                name: product.brand,
            } : undefined,
            offers: {
                '@type': 'Offer',
                price: product.price || 0,
                priceCurrency: product.currency || 'VND',
                availability: `https://schema.org/${product.availability || 'InStock'}`,
                url: window.location.href,
            },
        };

        if (product.rating && product.reviewCount) {
            schema.aggregateRating = {
                '@type': 'AggregateRating',
                ratingValue: product.rating,
                reviewCount: product.reviewCount,
                bestRating: 5,
            };
        }

        const script = document.createElement('script');
        script.type = 'application/ld+json';
        script.textContent = JSON.stringify(schema);
        script.id = 'product-jsonld';

        // Remove old one if exists
        const old = document.getElementById('product-jsonld');
        if (old) old.remove();

        document.head.appendChild(script);

        return () => {
            const el = document.getElementById('product-jsonld');
            if (el) el.remove();
        };
    }, [product]);
};

// ============ Organization Schema ============
export const useOrganizationSchema = () => {
    useEffect(() => {
        const schema = {
            '@context': 'https://schema.org',
            '@type': 'Organization',
            name: 'VNVT Store',
            url: SITE_URL,
            logo: `${SITE_URL}/vite.svg`,
            description: 'Hệ thống cửa hàng đồ gia dụng cao cấp, chính hãng. Miễn phí vận chuyển toàn quốc.',
            contactPoint: {
                '@type': 'ContactPoint',
                contactType: 'customer service',
                availableLanguage: 'Vietnamese',
            },
            sameAs: [],
        };

        const script = document.createElement('script');
        script.type = 'application/ld+json';
        script.textContent = JSON.stringify(schema);
        script.id = 'org-jsonld';

        const old = document.getElementById('org-jsonld');
        if (old) old.remove();

        document.head.appendChild(script);

        return () => {
            const el = document.getElementById('org-jsonld');
            if (el) el.remove();
        };
    }, []);
};

// ============ Breadcrumb Schema ============
interface BreadcrumbItem {
    name: string;
    url: string;
}

export const useBreadcrumbSchema = (items: BreadcrumbItem[]) => {
    useEffect(() => {
        if (!items.length) return;

        const schema = {
            '@context': 'https://schema.org',
            '@type': 'BreadcrumbList',
            itemListElement: items.map((item, i) => ({
                '@type': 'ListItem',
                position: i + 1,
                name: item.name,
                item: item.url.startsWith('http') ? item.url : `${SITE_URL}${item.url}`,
            })),
        };

        const script = document.createElement('script');
        script.type = 'application/ld+json';
        script.textContent = JSON.stringify(schema);
        script.id = 'breadcrumb-jsonld';

        const old = document.getElementById('breadcrumb-jsonld');
        if (old) old.remove();

        document.head.appendChild(script);

        return () => {
            const el = document.getElementById('breadcrumb-jsonld');
            if (el) el.remove();
        };
    }, [items]);
};

// ============ #85 SEO Head Management (Enhanced) ============
interface SEOProps {
    title: string;
    description?: string;
    canonicalPath?: string;
    ogImage?: string;
    ogType?: 'website' | 'article' | 'product';
    noindex?: boolean;
    keywords?: string;
}

export const useSEO = ({
    title,
    description,
    canonicalPath,
    ogImage,
    ogType = 'website',
    noindex = false,
    keywords,
}: SEOProps) => {
    useEffect(() => {
        const prevTitle = document.title;
        const fullTitle = `${title} | ${SITE_NAME}`;
        document.title = fullTitle;

        const currentUrl = canonicalPath
            ? `${SITE_URL}${canonicalPath}`
            : window.location.href.split('?')[0];

        // Meta description
        if (description) {
            setMetaTag('description', description);
        }

        // Keywords
        if (keywords) {
            setMetaTag('keywords', keywords);
        }

        // Robots
        if (noindex) {
            setMetaTag('robots', 'noindex, nofollow');
        } else {
            setMetaTag('robots', 'index, follow');
        }

        // Open Graph
        setMetaTag('og:title', fullTitle, true);
        setMetaTag('og:type', ogType, true);
        setMetaTag('og:url', currentUrl, true);
        setMetaTag('og:site_name', SITE_NAME, true);
        if (description) setMetaTag('og:description', description, true);
        if (ogImage || DEFAULT_OG_IMAGE) {
            const imgUrl = ogImage?.startsWith('http') ? ogImage : `${SITE_URL}${ogImage || DEFAULT_OG_IMAGE}`;
            setMetaTag('og:image', imgUrl, true);
        }

        // Twitter Card
        setMetaTag('twitter:card', ogImage ? 'summary_large_image' : 'summary');
        setMetaTag('twitter:title', fullTitle);
        setMetaTag('twitter:description', description || '');
        if (ogImage || DEFAULT_OG_IMAGE) {
            const imgUrl = ogImage?.startsWith('http') ? ogImage : `${SITE_URL}${ogImage || DEFAULT_OG_IMAGE}`;
            setMetaTag('twitter:image', imgUrl);
        }

        return () => {
            document.title = prevTitle;
            // Note: don't remove OG/Twitter tags on cleanup since index.html has defaults
        };
    }, [title, description, ogImage, ogType, noindex, keywords, canonicalPath]);

    useCanonical(canonicalPath);
};
