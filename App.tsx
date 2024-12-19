import React, {useState, useEffect, useCallback} from 'react';
import {Text, View, StyleSheet, Button, Alert} from 'react-native';

const App = () => {
  const [socket, setSocket] = useState<WebSocket | null>(null);
  const [connectionStatus, setConnectionStatus] = useState('Connecting...');
  const [receivedMessage, setReceivedMessage] = useState<string>('');

  const sendMessage = `
        ~~~~~~~~~~~~~~~~~~~~~~~~~
            SAM SO NAM'S KFC     
            456 Flavor Street       
            Edinburgh, EH2 2BB      
            Tel: 0131 765 4321      
        ~~~~~~~~~~~~~~~~~~~~~~~~~

Order No: #22588
Date: 19-Dec-2024      Time: 18:20
Server: Josh H.

Table: 3        Covers: 1
---------------------------------
Qty    Item                  Total
---------------------------------
55     Burgers               £79.99
55      Coke                  £12.49
55      Fries                 £19.96
---------------------------------
Subtotal:                   £72.90
VAT (20%):                  £14.58
---------------------------------
TOTAL:                      £87.48
---------------------------------

      Thank you for enjoying the best
      chicken and donuts in town!
      See you soon at Sam So Nam's!




  `;

  useEffect(() => {
    // Initialize WebSocket connection
    const webSocket = new WebSocket('ws://localhost:8080');

    // WebSocket lifecycle management
    webSocket.onopen = () => {
      setConnectionStatus('Connected to WebSocket');
      console.log('WebSocket connection opened.');
    };

    webSocket.onmessage = event => {
      console.log('Message from server:', event.data);
      setReceivedMessage(event.data);
    };

    webSocket.onerror = error => {
      console.error('WebSocket error:', error);
      setConnectionStatus('WebSocket error. Check connection.');
      Alert.alert('WebSocket Error', 'Unable to connect to the server.');
    };

    webSocket.onclose = () => {
      console.log('WebSocket connection closed.');
      setConnectionStatus('WebSocket connection closed');
    };

    setSocket(webSocket);

    return () => {
      console.log('Cleaning up WebSocket and console app.');
      webSocket.close();
      // Optional: Add logic to kill the console app process if needed
    };
  }, []);

  const sendMessageFunction = useCallback(() => {
    if (socket && socket.readyState === WebSocket.OPEN) {
      console.log(`Sending: ${sendMessage}`);
      socket.send(sendMessage);
    } else {
      console.error('WebSocket is not open. Unable to send message.');
      Alert.alert(
        'Error',
        'WebSocket is not connected. Unable to send the message.',
      );
    }
  }, [socket, sendMessage]);

  return (
    <View style={styles.container}>
      <Text style={styles.status}>{connectionStatus}</Text>
      <Text style={styles.label}>Last Received Message:</Text>
      <Text style={styles.message}>
        {receivedMessage || 'No messages received yet.'}
      </Text>
      <Button
        onPress={sendMessageFunction}
        title="Send Message"
        color="#007BFF"
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    padding: 16,
    backgroundColor: '#f5f5f5',
  },
  status: {
    fontSize: 16,
    fontWeight: 'bold',
    marginBottom: 16,
  },
  label: {
    fontSize: 14,
    marginBottom: 8,
    color: '#333',
  },
  message: {
    fontSize: 16,
    marginBottom: 16,
    padding: 10,
    backgroundColor: '#e9ecef',
    borderRadius: 4,
    color: '#495057',
  },
  input: {
    height: 40,
    borderColor: '#ced4da',
    borderWidth: 1,
    borderRadius: 4,
    paddingHorizontal: 10,
    marginBottom: 16,
  },
});

export default App;
