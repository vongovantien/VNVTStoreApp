export type UserRole = 'ADMIN' | 'MANAGER' | 'USER';
export type UserStatus = 'ACTIVE' | 'INACTIVE' | 'BANNED';

export interface User {
    id: string;
    fullName: string;
    email: string;
    avatar?: string;
    role: UserRole;
    status: UserStatus;
    lastLogin?: string;
    createdAt: string;
}

export interface UserPayload {
    fullName: string;
    email: string;
    role: UserRole;
    status: UserStatus;
    avatar?: string;
}
