using System;
using Fleck;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://127.0.0.1:8080");
            server.Start(socket =>
            {
                socket.OnOpen = () => Console.WriteLine("WebSocket connected.");
                socket.OnClose = () => Console.WriteLine("WebSocket disconnected.");
                socket.OnMessage = message =>
                {
                    Console.WriteLine("Message received: " + message);
                    socket.Send("Echo: " + message);

                    // Handle serial port interaction here if needed
                };
            });

            Console.WriteLine("WebSocket server running at ws://127.0.0.1:8080");
            Console.ReadLine(); // Keep the server running
        }
    }
}
