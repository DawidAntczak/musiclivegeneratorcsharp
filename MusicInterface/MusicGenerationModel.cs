using Newtonsoft.Json;
using System;
using WebSocketSharp;

namespace MusicInterface
{
    public class MusicGenerationModel : IDisposable
    {
        private readonly WsClient _wsClient;
        private readonly Action<string> _onLog;

        private Action<byte[]> _onReceived = _ => { };

        public MusicGenerationModel(string wsServerAddress, Action<string> onLog)
        {
            _wsClient = new WsClient(wsServerAddress);
            _onLog = onLog;
        }

        public MusicGenerationModel(string wsServerAddress) : this(wsServerAddress, _ => { }) { }

        public void Connect(Action<byte[]> onReceived)
		{
			_onReceived = onReceived;
			_wsClient.RegisterOnMessage(OnMessage);
			_wsClient.Connect();
			_wsClient.Send(new StartMessage());
		}

		public void Disconnect()
		{
			_wsClient.Send(new StopMessage());
			_wsClient.Disconnect();
			_wsClient.UnregisterOnMessage(OnMessage);
		}

		public void Request(ControlDataContract inputData)
		{
            _onLog($"Sending control data: {JsonConvert.SerializeObject(inputData)}" );
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
