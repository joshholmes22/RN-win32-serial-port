import {useState, useEffect} from 'react';
import {Text, View, TextInput, StyleSheet, Button} from 'react-native';

const App = () => {
  const [socket, setSocket] = useState<WebSocket | null>(null);
  const [message, setMessage] = useState('Connecting...');
  const [receivedMessage, setReceivedMessage] = useState('');
  const [sendMessage, setSendMessage] = useState('');

  useEffect(() => {
    const newSocket = new WebSocket('ws://localhost:8080');

    // Handle socket open event
    newSocket.onopen = () => {
      setMessage('Connected to WebSocket');
      console.log('WebSocket connection opened.');
    };

    // Handle incoming messages
    newSocket.onmessage = event => {
      console.log('Message from server:', event.data);
      setReceivedMessage(event.data);
    };

    // Handle socket errors
    newSocket.onerror = error => {
      console.error('WebSocket error:', error.message);
      setMessage('WebSocket error');
    };

    // Handle socket close event
    newSocket.onclose = () => {
      console.log('WebSocket connection closed.');
      setMessage('WebSocket connection closed');
    };

    setSocket(newSocket);

    return () => {
      newSocket.close();
    };
  }, []);

  const sendMessageFunction = () => {
    if (socket && socket.readyState === WebSocket.OPEN) {
      console.log(`Sending: ${sendMessage}`);
      socket.send(sendMessage);
      setSendMessage(''); // Clear the input after sending
    } else {
      console.error('WebSocket is not open. Unable to send message.');
    }
  };

  return (
    <View style={styles.container}>
      <Text>{message}</Text>
      <Text>Last Received Message: {receivedMessage}</Text>
      <TextInput
        style={styles.input}
        onChangeText={setSendMessage}
        value={sendMessage}
        placeholder="Type your message"
      />
      <Button
        onPress={sendMessageFunction}
        title="Send Message"
        color="#841584"
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    padding: 16,
  },
  input: {
    height: 40,
    margin: 12,
    borderWidth: 1,
    padding: 10,
  },
});

export default App;
