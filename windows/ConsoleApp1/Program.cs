using System;
using System.IO.Ports;
using Fleck;

class Program
{
    static void Main(string[] args)
    {
        // Configure and open the serial port
        SerialPort serialPort = new SerialPort("COM1", 9600)
        {
            Parity = Parity.None,
            StopBits = StopBits.One,
            DataBits = 8,
            Handshake = Handshake.None,
            DtrEnable = true,
            RtsEnable = true
        };

        try
        {
            serialPort.Open();
            Console.WriteLine("Serial port opened successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open serial port: {ex.Message}");
            return;
        }

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
                    // Write data to the serial port
                    serialPort.WriteLine(message);
                    Console.WriteLine($"Sent to serial port: {message}");

                    // Read response from the serial port
                    string response = serialPort.ReadLine();
                    Console.WriteLine($"Received from serial port: {response}");

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

        // Cleanup
        if (serialPort.IsOpen)
        {
            serialPort.Close();
            Console.WriteLine("Serial port closed.");
        }
    }
}
