using System.Text.Json;
using WebSocketSharp;

namespace DataTransporter
{
	public class WsClient : IDisposable
	{
		private readonly string _address;
		private readonly WebSocket _webSocket;

		public WsClient(string address)
		{
			_address = address;
			_webSocket = new WebSocket(_address);
		}

		public void Connect()
		{
			_webSocket.OnError += _webSocket_OnError;
			_webSocket.OnClose += _webSocket_OnClose;
			_webSocket.Connect();
			Console.WriteLine($"Client connected to {_address}");
		}

		public void Disconnect()
		{
			_webSocket.Close(CloseStatusCode.Normal);
			Console.WriteLine($"Client disconnected from {_address}");
			_webSocket.OnError -= _webSocket_OnError;
			_webSocket.OnClose -= _webSocket_OnClose;
		}

		private void _webSocket_OnClose(object? sender, CloseEventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void _webSocket_OnError(object? sender, WebSocketSharp.ErrorEventArgs e)
		{
			//throw new NotImplementedException();
		}

		public void Send<T>(T data)
		{
			var serializedData = JsonSerializer.Serialize(data);
			try
			{
				_webSocket.Send(serializedData);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public void RegisterOnMessage(EventHandler<MessageEventArgs> callback)
		{
			_webSocket.OnMessage += callback;
		}

		public void UnregisterOnMessage(EventHandler<MessageEventArgs> callback)
		{
			_webSocket.OnMessage -= callback;
		}

		public void Dispose()
		{
			((IDisposable)_webSocket).Dispose();
		}
	}
}
