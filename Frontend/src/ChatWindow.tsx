
import { useEffect, useRef, useState } from 'react';
import ChatInput from './ChatInput';
import { AIChatMessage, AIChatProtocolClient } from '@microsoft/ai-chat-protocol';

function ChatWindow() {
  const client = new AIChatProtocolClient('https://localhost:5002/api/chat')

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const handleSend = async (message: string) => {
    const userMessage: AIChatMessage = {
      content: message,
      role: 'user',
    };
    setMessages([...messages, userMessage ]);
    const stream = await client.getStreamedCompletion([userMessage]);
    let responseContent: string = '';
    for await (const response of stream) {
      responseContent += response.delta?.content ?? '';
      setMessages([...messages, userMessage, { content: responseContent, role: 'assistant' }]);
    }
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