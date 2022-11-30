using Server.Interfaces;
using Common.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Server.Services
{
    public sealed class CustomConnectionManager : IConnectionManager
    {
        private readonly ILogger<CustomConnectionManager> logger;

        private readonly ConcurrentDictionary<string, WebSocket> connections =
            new ConcurrentDictionary<string, WebSocket>();

        public CustomConnectionManager(ILogger<CustomConnectionManager> logger)
        {
            this.logger = logger;
        }

        public string AddSocket(WebSocket socket)
        {
            var connectiodId = GetNewConnectionId();

            connections.TryAdd(connectiodId, socket);

            return connectiodId;
        }

        public Task RemoveSocket(string id)
        {
            connections.TryRemove(id, out var socket);

            return socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                    "Closed by server",
                                    CancellationToken.None);
        }

        public async Task ProcessMessage(byte[] messageBytes, string sender)
        {
            try
            {
                var str = Encoding.UTF8.GetString(messageBytes);

                var message = JsonSerializer.Deserialize<ChatMessage>(messageBytes);

                if (message != null && message.Command == "SendToOthers")
                    await SendMessageToOthers(message, sender);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cannot process message");
            }
        }

        private async Task SendMessageToOthers(ChatMessage message, string sender)
        {
            var otherClients = connections.Where(x => x.Key != sender).Select(x => x.Value);

            message.Command = "Send";
            message.CreatedAt = DateTime.Now.ToString("HH:mm:ss");
            var arrayBuffer = new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(message));

            foreach (var client in otherClients)
            {
                try
                {
                    await client.SendAsync(buffer: arrayBuffer,
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Cannot send message");
                }
            }
        }

        private string GetNewConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
