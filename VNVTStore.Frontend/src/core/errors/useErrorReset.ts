import { useCallback, useState } from 'react';

export const useErrorReset = (_initialState: unknown = null) => {
    const [key, setKey] = useState(0);

    const reset = useCallback(() => {
        setKey((prev) => prev + 1);
    }, []);

    return {
        resetKey: key,
        reset,
    };
};
