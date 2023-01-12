using Newtonsoft.Json;
using System;
using WebSocketSharp;

namespace MusicInterface
{
    public class MusicReceiver : IDisposable
    {
        private readonly WsClient _wsClient;
        private readonly Action<string> _onLog;

        private Action<byte[]> _onReceived = _ => { };

        public MusicReceiver(WsClient wsClient, Action<string> onLog)
        {
            _wsClient = wsClient;
            _onLog = onLog;
        }

        public MusicReceiver(WsClient wsClient) : this(wsClient, _ => { }) { }

        public void StartListening(Action<byte[]> onReceived)
		{
			_onReceived = onReceived;
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

		public void SendControls(ControlDataContract inputData)
		{
            _onLog($"Sending input: {JsonConvert.SerializeObject(inputData)}" );
			_wsClient.Send(inputData);
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
            _onLog($"Received data of length: {e.RawData.Length} bytes.");
			_onReceived(e.RawData);
		}

		public void Dispose()
		{
			((IDisposable)_wsClient).Dispose();
		}
	}
}
