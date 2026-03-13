export interface BaseResponse<T> {
    data: T;
    message: string;
    status: number;
    success: boolean;
}

export interface ApiError {
    message: string;
    code: string;
    status: number;
    errors?: Record<string, string[]>;
}
