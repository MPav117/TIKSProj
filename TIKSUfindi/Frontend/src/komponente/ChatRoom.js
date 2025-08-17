import React, { useState, useContext, useEffect, useRef } from 'react';
import { AppContext } from "../App";

const ChatRoom = ({ messages, sendMessage, onClose }) => {
  const [message, setMessage] = useState("");
  const { korisnik } = useContext(AppContext);
  const messagesEndRef = useRef(null);
  const jezik = useContext(AppContext).jezik

  console.log(korisnik.username);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (message.trim() !== "") {
      sendMessage(message);
      setMessage("");
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  return (
    <div className="flex justify-center items-center h-screen pozadina bg-gradient-to-r from-transparent via-gray-100 to-transparent opacity-80 z-10">
      <div className="backdrop-blur-sm bg-white bg-opacity-60 shadow-lg w-full max-w-lg h-3/4 flex flex-col rounded-lg border border-amber-900 relative">
        <div className='flex justify-end p-2'>
        <button 
          onClick={onClose} 
          className="bg-amber-600 text-white font-bold rounded px-2 py-1 mr-1"
        >
          X
        </button>
        </div>
        <div className="flex-1 p-4 overflow-y-auto">
          {messages.map((msg, index) => (
            <div 
              key={index} 
              className={`mb-3 flex ${
                msg.senderUsername === korisnik.username ? 'justify-end' : 'justify-start'
              }`}
            >
              <div className={`p-3 rounded-lg shadow max-w-max ${
                msg.senderUsername === korisnik.username ? 'bg-orange-600 bg-opacity-60 text-right font-semibold' : 'bg-orange-300 bg-opacity-60 text-left font-semibold'
              }`}>
                <strong>{msg.senderUsername}:</strong> {msg.message}
              </div>
            </div>
          ))}
          <div ref={messagesEndRef} />
        </div>
        <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row p-4 border-t border-gray-300">
            <input
              type="text"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder={jezik.chat.prompt}
              className="flex-1 mb-2 md:mb-0 md:mr-2 p-2 border rounded"
            />
            <button 
              type="submit" 
              className="rounded px-4 py-2 bg-yellow-700 bg-opacity-50 text-yellow-900 font-bold border border-yellow-800 hover:bg-yellow-700 hover:border-yellow-700 hover:text-white transition-colors duration-300"
            >
              {jezik.chat.send}
            </button>
          </form>

      </div>
    </div>

  );
};

export default ChatRoom;
