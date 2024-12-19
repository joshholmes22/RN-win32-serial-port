using System;
using System.IO.Ports;
using System.ServiceProcess;
using Fleck;
using System.Text;
using System.Linq;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        // Check if the app is running as a console or service
        if (Environment.UserInteractive)
        {
            // For manual debugging or testing, run as a console app
            Console.WriteLine("Running as a console application...");
            var service = new SerialPortService();
            service.Start(); // Simulate service start
            Console.WriteLine("Press Ctrl+C to exit...");
            Thread.Sleep(Timeout.Infinite);
        }
        else
        {
            // Run as a Windows Service
            ServiceBase.Run(new SerialPortService());
        }
    }
}

public class SerialPortService : ServiceBase
{
    private static SerialPort _serialPort;
    private static WebSocketServer _server;

    public SerialPortService()
    {
        ServiceName = "SerialPortService";
    }

    protected override void OnStart(string[] args)
    {
        Console.WriteLine("Service starting...");
        Start();
    }

    public void Start()
    {
        Console.WriteLine("Initializing WebSocket server...");
        _server = new WebSocketServer("ws://0.0.0.0:8080");
        _server.Start(socket =>
        {
            socket.OnOpen = () => Console.WriteLine("WebSocket connected.");
            socket.OnClose = () => Console.WriteLine("WebSocket disconnected.");
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

        Console.WriteLine("WebSocket server running.");
    }

    protected override void OnStop()
    {
        Console.WriteLine("Service stopping...");
        _server.Dispose();
        _serialPort?.Close();
    }

    private static string InitiatePrintProcess(string message)
    {

        byte[] buffer = Encoding.ASCII.GetBytes(message);

        if (_serialPort == null)
        {
            InitializeSerialPort();
        }

        lock (_serialPort)
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
            PortName = ports.FirstOrDefault(),
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
