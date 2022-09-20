using NetCoreServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChatServer
{
    class ChatSession : TcpSession
    {
        public ChatSession(TcpServer server) : base(server) { }

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} connected!");

            // Send invite message
            //string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            //SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

            message = message.Replace(((char)2).ToString(), "").Replace(((char)3).ToString(), "");

            Console.WriteLine("Incoming: " + message);

            // Multicast message to all connected sessions

            if (message.StartsWith("N"))
            {
                string ACK = ((char)6).ToString();
                string NACK = ((char)15).ToString();
                Server.Multicast(ACK);
            }



            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class ChatServer : TcpServer
    {
        public ChatServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() { return new ChatSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TCP server port
            int port = 1111;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            Console.WriteLine($"TCP server port: {port}");

            Console.WriteLine();

            // Create a new TCP chat server
            var server = new ChatServer(IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }
                if (line == "+")
                {
                    Console.Write("Send report to client...");
                    server.Multicast(((char)2).ToString() + "DXDF-260                       +001475001+00000092+00001401.500+00000001.500+00000001.000+00000000.300+00000001+00000002+00000003+00000004+00000005+00000006+00000007+00000008+00000009+00000010+00000011+00000012+00000013+00000014+00000015+00000016+00000017+00000018+00000019+00000020" + ((char)3).ToString());
                    Console.WriteLine("Send report done!");
                    continue;
                }
                // Multicast admin message to all sessions
                line = "(admin) " + line;
                //server.Multicast(line);
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
    }
}
