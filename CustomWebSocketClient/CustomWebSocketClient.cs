using Common.Models;
using System.Net.WebSockets;
using System.Text.Json;

namespace CustomWebSocketClientProtocol
{
    public class CustomWebSocketClient
    {
        private readonly ClientWebSocket webSocket = new ClientWebSocket();

        private readonly Dictionary<string, (Type, Action<object>)> handlers =
            new Dictionary<string, (Type, Action<object>)>();

        public WebSocketState State => webSocket.State;

        public async Task ConnectAsync(string address, CancellationToken? cancellationToken = null)
        {
            await webSocket.ConnectAsync(
                new Uri(address),
                cancellationToken ?? CancellationToken.None);

            if (webSocket.State == WebSocketState.Open)
            {
                Task.Run(Listen);
            }
        }

        public Task StopAsync()
        {
            return webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closed by client",
                CancellationToken.None);
        }

        public Task SendAsync<T>(string command, T message)
        {
            var webSocketMessage = new ChatMessage
            {
                Command = command,
                Body = JsonSerializer.Serialize(message)
            };

            var buffer = new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(webSocketMessage));

            return webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task DisposeAsync()
        {
            await Task.Run(DisposeWebSocket);
        }

        private void DisposeWebSocket()
        {
            this.webSocket.Dispose();
        }

        private async Task Listen()
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text && receiveResult.EndOfMessage)
                {
                    try
                    {
                        Handle(buffer.Array[0..(receiveResult.Count)]);
                    }
                    catch
                    {
                        //ignored
                    }
                }
            }
        }

        private void Handle(byte[] buffer)
        {
            var message = JsonSerializer.Deserialize<ChatMessage>(buffer);

            if (message == null)
                return;

            var messageHandlers = handlers.Where(x => x.Key == message.Command).ToList();

            if (messageHandlers.Count == 0)
                return;

            foreach (var handler in messageHandlers)
            {
                handler.Value.Item2(JsonSerializer.Deserialize(message.Body, handler.Value.Item1));
            }
        }

        public void On<T>(string name, Action<T> handler) where T : class
        {
            handlers.Add(name, (typeof(T), new Action<object>(param => handler((T)param))));
        }
    }
}