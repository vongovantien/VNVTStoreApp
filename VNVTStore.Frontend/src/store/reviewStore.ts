import { create } from 'zustand';

interface ReviewStore {
  refreshTrigger: number;
  triggerRefresh: () => void;
}

export const useReviewStore = create<ReviewStore>((set) => ({
  refreshTrigger: 0,
  triggerRefresh: () => set((state) => ({ refreshTrigger: state.refreshTrigger + 1 })),
}));
