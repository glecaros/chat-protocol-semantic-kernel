
import { useEffect, useRef, useState } from 'react';
import ChatInput from './ChatInput';
import { AIChatCompletionDelta, AIChatMessage, AIChatProtocolClient } from '@microsoft/ai-chat-protocol';

async function* getContent(deltas: AsyncIterable<AIChatCompletionDelta>): AsyncIterable<string> {
  for await (const delta of deltas) {
    if (delta?.delta?.content && delta?.delta?.content !== '') {
      yield delta.delta.content;
    }
  }
}

function ChatWindow() {
  const client = new AIChatProtocolClient('/api/chat', { allowInsecureConnection: true })

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const handleSend = async (message: string) => {
    const userMessage: AIChatMessage = {
      content: message,
      role: 'user',
    };
    setMessages((prevMessages) => [...prevMessages, userMessage]);
    const stream = await client.getStreamedCompletion([userMessage]);

    let responseMessage: AIChatMessage = {
      content: '',
      role: 'assistant',
    };
    const updateMessages = [...messages, userMessage];
    for await (const response of getContent(stream)) {
      responseMessage.content += response;
      setMessages((_) => [...updateMessages, responseMessage]);
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