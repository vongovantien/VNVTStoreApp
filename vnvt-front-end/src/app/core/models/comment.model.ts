export interface Comment {
  id: number;
  user: string;
  avatarUrl: string;
  text: string;
  rating: number;
  showReplyForm?: boolean;
  replyText?: string;
  replies?: Comment[];
}
