
import { useEffect, useRef, useState } from 'react';
import ChatInput from './ChatInput';
import { AIChatMessage } from '@microsoft/ai-chat-protocol';

function ChatWindow() {
  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const handleSend = (message: string) => {
    setMessages([...messages, { content: message, role: 'user' }]);
  };

  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [messages]);


  return (
    <div className='chat-window'>
      <div className='chat-history'>
        {messages.map((message, index) => (
          <div key={index} className={message.role}>
            <p>{message.content}</p>
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      <ChatInput className="chat-input" onSend={handleSend} />
    </div>
  );
}

export default ChatWindow;