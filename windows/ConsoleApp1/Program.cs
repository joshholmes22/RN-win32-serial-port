using System;
using System.IO.Ports;
using Fleck;
using System.Text;
using System.Linq;
using System.Threading;

class Program
{
    private static SerialPort _serialPort;
    private static readonly object _serialPortLock = new object();
    private static WebSocketServer _server;

    static void Main(string[] args)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Shutting down gracefully...");
            _server.Dispose(); // Gracefully dispose WebSocket server
            _serialPort?.Close(); // Close the serial port if open
            Environment.Exit(0);
        };

        // Start the WebSocket server
        _server = new WebSocketServer("ws://0.0.0.0:8080");
        _server.Start(socket =>
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

                if (string.IsNullOrWhiteSpace(message))
                {
                    socket.Send("Invalid or empty message.");
                    return;
                }

                try
                {
                    string result = InitiatePrintProcess(message);
                    socket.Send(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during serial communication: {ex.Message}");
                    socket.Send($"Error: {ex.Message}");
                }
            };
        });

        Console.WriteLine("WebSocket server running on ws://localhost:8080");
        Console.WriteLine("Press Ctrl+C to exit...");
        Thread.Sleep(Timeout.Infinite);
    }
    public static string InitiatePrintProcess(string message)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);

        lock (_serialPortLock)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    InitializeSerialPort(); // Initialize and open serial port
                }

                _serialPort.Write(buffer, 0, buffer.Length); // Send data to serial port
                Console.WriteLine("Data sent to serial port.");
                return "Data successfully sent to the printer.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial port error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }

    private static void InitializeSerialPort()
    {
        var ports = SerialPort.GetPortNames();

        if (ports.Length == 0)
        {
            throw new InvalidOperationException("No serial ports available on this machine.");
        }

        _serialPort = new SerialPort
        {
            PortName = ports.FirstOrDefault(), // Use the first available port
            BaudRate = 9600,
            Parity = Parity.Even,
            StopBits = StopBits.One,
            DataBits = 8,
            Handshake = Handshake.RequestToSendXOnXOff,
            Encoding = Encoding.UTF8,
            ReadTimeout = 2000,
            WriteTimeout = 2000,
            NewLine = "\r\n"
        };

        _serialPort.Open();
        Console.WriteLine($"Serial port {_serialPort.PortName} opened.");
    }
}
