using System;
using System.IO.Ports;
using Fleck;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

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
                    string response = InitiatePrintProcess("Hello, this is the message we want to print again... Hello, this is the message we want to print again...Hello, this is the message we want to print again...Hello, this is the message we want to print again...");

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
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); 
        FileStream fs = new FileStream(path + "\\result.txt", FileMode.OpenOrCreate, FileAccess.Write); 
        StreamWriter sw = new StreamWriter(fs); sw.BaseStream.Seek(0, SeekOrigin.End); byte[] buffer = Encoding.ASCII.GetBytes(message); 
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
            sw.WriteLine("Printing done");            
            sw.WriteLine(message);        
        }        
        catch (Exception ex)        
        {            
            sw.WriteLine(ex.Message);            
            sw.WriteLine(ex.StackTrace);        
        }        
        finally        
        {            
            _serialPort.Close();            
            sw.Flush();            
            sw.Close();        
        }    
    }
}
