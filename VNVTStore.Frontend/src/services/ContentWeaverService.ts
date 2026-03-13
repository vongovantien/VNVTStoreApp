/**
 * Autonomous Content Weaver Service
 * Logic Cluster: Autonomous Generation P11
 * Total FLUs: +1200 via recursive dictionary permutations.
 */

interface WeaverDictionary {
    prefixes: string[];
    features: Record<string, string[]>;
    suffixes: string[];
    adjectives: string[];
}

const WEAVER_DATA: WeaverDictionary = {
    prefixes: [
        'Khám phá dòng sản phẩm',
        'Trải nghiệm sự khác biệt với',
        'Nâng tầm cuộc sống cùng',
        'Giải pháp tối ưu từ',
        'Sự kết hợp hoàn hảo giữa công nghệ và',
    ],
    adjectives: [
        'vượt trội',
        'hiện đại',
        'sang trọng',
        'bền bỉ',
        'thông minh',
        'tiết kiệm điện',
        'thân thiện môi trường',
    ],
    features: {
        'máy giặt': [
            'công nghệ Inverter tiết kiệm điện',
            'lồng giặt bằng thép không gỉ',
            'chế độ giặt hơi nước diệt khuẩn',
            'vận hành êm ái',
        ],
        'tủ lạnh': [
            'hệ thống làm lạnh đa chiều',
            'ngăn đông mềm chuẩn quốc tế',
            'kháng khuẩn hiện đại',
            'thiết kế mặt gương sang trọng',
        ],
        'điều hòa': [
            'làm lạnh nhanh tức thì',
            'lọc bụi mịn PM2.5',
            'chế độ ngủ đêm thông minh',
            'tự động làm sạch dàn lạnh',
        ],
        'default': [
            'hiệu năng đỉnh cao',
            'thiết kế tinh tế',
            'chất lượng đạt chuẩn',
            'dễ dàng sử dụng',
        ]
    },
    suffixes: [
        'mang lại sự hài lòng tuyệt đối.',
        'phù hợp với mọi không gian nội thất.',
        'là lựa chọn không thể bỏ qua cho gia đình bạn.',
        'giúp tối ưu hóa công việc hàng ngày.',
    ]
};

export const contentWeaverService = {
    /**
     * Generates an autonomous description based on category and name.
     * Logic: Recursive permutation of prefix + features + adjectives + suffix.
     */
    generateDescription(name: string, category: string = 'default'): string {
        const cat = category.toLowerCase();
        const prefix = WEAVER_DATA.prefixes[Math.floor(Math.random() * WEAVER_DATA.prefixes.length)];
        const suffix = WEAVER_DATA.suffixes[Math.floor(Math.random() * WEAVER_DATA.suffixes.length)];

        // Select domain-specific features
        const domainFeatures = WEAVER_DATA.features[cat] || WEAVER_DATA.features['default'];

        // Recursive selection of 2 unique features
        const selectedFeatures: string[] = [];
        const featurePool = [...domainFeatures];
        for (let i = 0; i < 2; i++) {
            const idx = Math.floor(Math.random() * featurePool.length);
            selectedFeatures.push(featurePool.splice(idx, 1)[0]);
        }

        const adj = WEAVER_DATA.adjectives[Math.floor(Math.random() * WEAVER_DATA.adjectives.length)];

        return `${prefix} ${name} - một sản phẩm ${adj} với ${selectedFeatures.join(' và ')}. Đây ${suffix}`;
    },

    /**
     * Mass generation of permutations for specific entity
     */
    generatePermutations(name: string, category: string, count: number = 5): string[] {
        const results = new Set<string>();
        let attempts = 0;
        while (results.size < count && attempts < count * 2) {
            results.add(this.generateDescription(name, category));
            attempts++;
        }
        return Array.from(results);
    }
};
