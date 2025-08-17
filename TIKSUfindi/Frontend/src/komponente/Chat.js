//npm install @microsoft/signalr

import React, { useState, useEffect, useContext } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import ChatRoom from './ChatRoom';
import { AppContext } from "../App";

const Chat = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const chatRoom = location.state?.chatRoom;
  const [conn, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
  const korisnik = useContext(AppContext).korisnik;
  const jezik = useContext(AppContext).jezik

  useEffect(() => {

    const connectToHub = async () => {
      try {
        const connection = new HubConnectionBuilder()
          .withUrl("https://localhost:7080/chat")
          .configureLogging(LogLevel.Information)
          .build();

        connection.on("ReceiveMessage", (username, msg) => {
          setMessages(messages => [...messages, { senderUsername: username, message: msg }]);
        });

        connection.on('LoadMessages', (loadedMessages) => {
          setMessages(loadedMessages);
        });

        connection.on('Error', (errorMessage) => {
          console.error("Server error:", errorMessage);
          alert(errorMessage);
        });

        await connection.start();

        await connection.invoke("JoinSpecificChatRoom", chatRoom );
        await connection.invoke('LoadMessages', chatRoom);

        setConnection(connection);
      } catch (e) {
        window.alert(jezik.general.error.netGreska + ": " + e.message)
      }
    };

    if (!conn) {
      connectToHub();
    }

    return () => {
      if (conn) {
        conn.stop();
      }
    };
  }, [chatRoom, conn]);

  const sendMessage = async (message) => {
    const drugaStranaID = chatRoom.split('_').find(id => id !== korisnik.id.toString());
    const senderId = korisnik.id;
    const receiverId = parseInt(drugaStranaID);

    try {
      await conn.invoke("SendMessage",  chatRoom , message, senderId, receiverId);
    } catch (e) {
      window.alert(jezik.general.error.netGreska + ": " + e.message)
    }
  };

  const handleClose = () => {
    navigate('/profile');
  };

  return (
    <div>
      <ChatRoom messages={messages} sendMessage={sendMessage} username={korisnik.username} onClose={handleClose}></ChatRoom>
    </div>
  );
};

export default Chat;