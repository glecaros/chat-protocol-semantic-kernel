import React, { useState } from 'react';

interface ChatInputProps {
  onSend: (input: string) => void;
  className?: string;
}

function ChatInput({ onSend, className }: ChatInputProps) {
  const [input, setInput] = useState('');

  const handleSend = () => {
    if (input.trim() !== '') {
      onSend(input);
      setInput('');
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter') {
      handleSend();
      event.preventDefault();
    }
  };

  return (
    <div className={className}>
      <input value={input} onChange={e => setInput(e.target.value)} onKeyDown={handleKeyPress} />
      <button onClick={handleSend}>Send</button>
    </div>
  );
}


export default ChatInput;
