import { createEntityService } from '@/services/baseService';
import { User, UserPayload } from '../types/User';

export const userService = {
    ...createEntityService<User, UserPayload, Partial<UserPayload>>({
        endpoint: '/users',
        resourceName: 'User'
    })
};

export default userService;
