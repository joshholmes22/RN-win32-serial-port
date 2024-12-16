using System;
using System.IO.Ports;
using Fleck;
using System.Text;

class Program
{
    static SerialPort _serialPort;
    static void Main(string[] args)
    {
        // Start the WebSocket server
        var server = new WebSocketServer("ws://0.0.0.0:8080");
        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Console.WriteLine("WebSocket connected.");
            };

            socket.OnClose = () =>
            {
                Console.WriteLine("WebSocket disconnected.");
            };

            socket.OnMessage = message =>
            {
                Console.WriteLine($"Received from WebSocket: {message}");

                try
                {
                    InitiatePrintProcess(message);

                    string response = "Still going, possbily printed?";
                    // Send the response back to the WebSocket client
                    socket.Send(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during serial communication: {ex.Message}");
                    socket.Send($"Error: {ex.Message}");
                }
            };
        });

        Console.WriteLine("WebSocket server running on ws://localhost:8080");
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
    public static void InitiatePrintProcess(string message) 
    { 
        byte[] buffer = Encoding.ASCII.GetBytes(message); 
        try 
        { 
            _serialPort = new SerialPort(); 
            var ports = SerialPort.GetPortNames(); 
            _serialPort.PortName = ports[0];
            _serialPort.BaudRate = 9600; 
            _serialPort.Parity = Parity.Even; 
            _serialPort.StopBits = StopBits.One; 
            _serialPort.Handshake = Handshake.RequestToSendXOnXOff; 
            _serialPort.DataBits = 8; 
            _serialPort.Encoding = Encoding.UTF8; 
            _serialPort.Open(); 

            _serialPort.ReadTimeout = 2000;            
            _serialPort.WriteTimeout = 2000;            
            _serialPort.NewLine = "\r\n";            
            _serialPort.Write(buffer, 0, buffer.Length);    
        }        
        catch (Exception ex)        
        {
            Console.WriteLine(ex.ToString());
        }        
        finally        
        {            
            _serialPort.Close();     
        }    
    }
}
