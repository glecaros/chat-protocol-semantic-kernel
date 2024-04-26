
import { useEffect, useRef, useState } from 'react';
import ChatInput from './ChatInput';
import { AIChatMessage, AIChatMessageDelta, AIChatProtocolClient } from '@microsoft/ai-chat-protocol';

function ChatWindow() {
  const client = new AIChatProtocolClient('/api/chat', { allowInsecureConnection: true })

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const [sessionState, setSessionState] = useState<unknown | undefined>(undefined);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const handleSend = async (message: string) => {
    const userMessage: AIChatMessage = {
      content: message,
      role: 'user',
    };

    const updatedMessages = [...messages, userMessage];

    setMessages((_) => [...updatedMessages]);

    const responseMessage: AIChatMessage = {
      content: '',
      role: 'assistant',
    };

    const updateMessages = (delta: AIChatMessageDelta) => {
      responseMessage.content += delta.content;
      setMessages((_) => [...updatedMessages, responseMessage]);
    };

    const stream = await client.getStreamedCompletion([userMessage], {
      sessionState: sessionState,
    });

    for await (const response of stream) {
      if (response.sessionState) {
        setSessionState(response.sessionState);
      }
      if (response.delta?.content) {
        updateMessages(response.delta);
      }
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
