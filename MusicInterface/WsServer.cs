using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace MusicInterface
{

    public class WsServer : WebSocketBehavior
    {
        private readonly string _address;
        private readonly WebSocketServer _server;

        public WsServer(int port = 7891)
        {
            _address = $"ws://localhost:{port}";
            _server = new WebSocketServer(_address);
        }

        public void Start()
        {
            _server.Start();
            Console.WriteLine($"Server starter at {_address}");
        }

        public void Stop()
        {
            _server.Stop();
            Console.WriteLine($"Server stopped");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Received message from client: " + e.Data);
            Send("Server response: " + e.Data);
        }

        protected override void OnOpen()
        {
        }

        protected override void OnClose(CloseEventArgs e)
        {
        }

        protected override void OnError(ErrorEventArgs e)
        {
        }
    }
}