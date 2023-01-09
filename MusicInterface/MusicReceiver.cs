using Newtonsoft.Json;
using System;
using WebSocketSharp;

namespace MusicInterface
{
	public class MusicReceiver : IDisposable
	{
		private readonly WsClient _wsClient;
		private Action<byte[]> _onReceived = _ => { };

		public MusicReceiver(WsClient wsClient)
		{
			_wsClient = wsClient;
		}

		public void StartListening(Action<byte[]> onReveived)
		{
			_onReceived = onReveived;
			_wsClient.RegisterOnMessage(OnMessage);
			_wsClient.Connect();
			_wsClient.Send(new StartMessage());
		}

		public void StopListening()
		{
			_wsClient.Send(new StopMessage());
			_wsClient.Disconnect();
			_wsClient.UnregisterOnMessage(OnMessage);
		}

		public void SendInput(InputData inputData)
		{
			Console.WriteLine($"Sending input: {JsonConvert.SerializeObject(inputData)}" );
			_wsClient.Send(inputData);
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			Console.WriteLine($"Received data of length: {e.RawData.Length} bytes.");
			_onReceived(e.RawData);
		}

		public void Dispose()
		{
			((IDisposable)_wsClient).Dispose();
		}
	}
}
