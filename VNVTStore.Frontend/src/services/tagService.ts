import { createEntityService } from './baseService';
import { Tag } from '@/types';

export const tagService = createEntityService<Tag>({
    endpoint: '/tags',
    resourceName: 'Tag'
});
