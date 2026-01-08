import { memo, useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { MessageCircle, X, Send, Bot, User, Minimize2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';
import type { ChatMessage } from '@/types';

export const ChatWidget = memo(() => {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [isMinimized, setIsMinimized] = useState(false);
  const [message, setMessage] = useState('');
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: '1',
      role: 'assistant',
      content: 'Xin chào! 👋 Tôi là trợ lý AI của VNVT Store. Tôi có thể giúp gì cho bạn?',
      timestamp: new Date().toISOString(),
    },
  ]);
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSend = async () => {
    if (!message.trim()) return;

    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: message,
      timestamp: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setMessage('');
    setIsTyping(true);

    // Simulate AI response (replace with actual AI integration)
    setTimeout(() => {
      const responses = [
        'Cảm ơn bạn đã liên hệ! Bạn có thể cho tôi biết thêm về sản phẩm bạn đang tìm kiếm không?',
        'Tôi có thể giúp bạn tìm sản phẩm phù hợp. Bạn đang quan tâm đến danh mục nào?',
        'Chúng tôi hiện đang có nhiều chương trình khuyến mãi hấp dẫn. Bạn có muốn tôi giới thiệu không?',
        'Để hỗ trợ bạn tốt hơn, bạn có thể liên hệ hotline 1900 123 456 hoặc chat Zalo nhé!',
      ];

      const aiMessage: ChatMessage = {
        id: Date.now().toString(),
        role: 'assistant',
        content: responses[Math.floor(Math.random() * responses.length)],
        timestamp: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, aiMessage]);
      setIsTyping(false);
    }, 1500);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <>
      {/* Chat Button */}
      <motion.button
        className={cn(
          'fixed bottom-6 right-6 z-50 w-14 h-14 rounded-full shadow-lg flex items-center justify-center transition-colors',
          isOpen ? 'bg-gray-600' : 'bg-gradient-to-r from-primary to-purple-500 hover:shadow-xl'
        )}
        onClick={() => setIsOpen(!isOpen)}
        whileHover={{ scale: 1.05 }}
        whileTap={{ scale: 0.95 }}
      >
        {isOpen ? <X size={24} className="text-white" /> : <MessageCircle size={24} className="text-white" />}
      </motion.button>

      {/* Chat Window */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 20, scale: 0.9 }}
            animate={{ 
              opacity: 1, 
              y: 0, 
              scale: 1,
              height: isMinimized ? 'auto' : 500,
            }}
            exit={{ opacity: 0, y: 20, scale: 0.9 }}
            className="fixed bottom-24 right-6 z-50 w-96 max-w-[calc(100vw-3rem)] bg-primary rounded-2xl shadow-2xl overflow-hidden flex flex-col"
          >
            {/* Header */}
            <div className="bg-gradient-to-r from-primary to-purple-500 text-white p-4 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-white/20 rounded-full flex items-center justify-center">
                  <Bot size={20} />
                </div>
                <div>
                  <h3 className="font-semibold">AI Assistant</h3>
                  <p className="text-xs text-white/80">Online • Phản hồi nhanh</p>
                </div>
              </div>
              <button
                onClick={() => setIsMinimized(!isMinimized)}
                className="p-2 hover:bg-white/20 rounded-lg transition-colors"
              >
                <Minimize2 size={18} />
              </button>
            </div>

            {!isMinimized && (
              <>
                {/* Messages */}
                <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-secondary">
                  {messages.map((msg) => (
                    <div
                      key={msg.id}
                      className={cn('flex gap-3', msg.role === 'user' ? 'flex-row-reverse' : '')}
                    >
                      <div
                        className={cn(
                          'w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0',
                          msg.role === 'user'
                            ? 'bg-primary text-white'
                            : 'bg-gradient-to-r from-primary to-purple-500 text-white'
                        )}
                      >
                        {msg.role === 'user' ? <User size={16} /> : <Bot size={16} />}
                      </div>
                      <div
                        className={cn(
                          'max-w-[75%] px-4 py-2 rounded-2xl',
                          msg.role === 'user'
                            ? 'bg-primary text-white rounded-br-md'
                            : 'bg-primary shadow-sm rounded-bl-md'
                        )}
                      >
                        <p className="text-sm">{msg.content}</p>
                      </div>
                    </div>
                  ))}

                  {/* Typing indicator */}
                  {isTyping && (
                    <div className="flex gap-3">
                      <div className="w-8 h-8 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center">
                        <Bot size={16} className="text-white" />
                      </div>
                      <div className="bg-primary shadow-sm px-4 py-2 rounded-2xl rounded-bl-md">
                        <div className="flex gap-1">
                          <span className="w-2 h-2 bg-tertiary rounded-full animate-bounce" />
                          <span className="w-2 h-2 bg-tertiary rounded-full animate-bounce [animation-delay:0.2s]" />
                          <span className="w-2 h-2 bg-tertiary rounded-full animate-bounce [animation-delay:0.4s]" />
                        </div>
                      </div>
                    </div>
                  )}

                  <div ref={messagesEndRef} />
                </div>

                {/* Input */}
                <div className="p-4 border-t bg-primary">
                  <div className="flex gap-2">
                    <input
                      type="text"
                      value={message}
                      onChange={(e) => setMessage(e.target.value)}
                      onKeyPress={handleKeyPress}
                      placeholder="Nhập tin nhắn..."
                      className="flex-1 px-4 py-2 border rounded-full text-sm focus:outline-none focus:border-primary"
                    />
                    <Button
                      onClick={handleSend}
                      disabled={!message.trim()}
                      rounded
                      size="sm"
                      className="w-10 h-10 p-0"
                    >
                      <Send size={18} />
                    </Button>
                  </div>
                  <p className="text-xs text-tertiary text-center mt-2">
                    Powered by AI • VNVT Store
                  </p>
                </div>
              </>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
});

ChatWidget.displayName = 'ChatWidget';

export default ChatWidget;
