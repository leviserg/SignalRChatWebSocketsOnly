using Server.Services;
using System.Net.WebSockets;

namespace Server.Middleware
{
    public class CustomWebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly CustomConnectionManager connectionManager;

        public CustomWebSocketMiddleware(RequestDelegate next, CustomConnectionManager connectionManager)
        {
            this.next = next;
            this.connectionManager = connectionManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest
                && context.Request.Path == "/messages")
            {
                //string userName = (context.Request.Query.ContainsKey("username")) ? context.Request.Query["username"].First() : "Anonymous";

                var socket = await context.WebSockets.AcceptWebSocketAsync();

                var id = connectionManager.AddSocket(socket);

                await Loop(socket, id);

                await connectionManager.RemoveSocket(id);
            }
            else
            {
                await next(context);
            }
        }

        private async Task Loop(WebSocket socket, string connectionId)
        {
            while (socket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.EndOfMessage && result.Count <= 1024)
                {
                    await connectionManager.ProcessMessage(buffer.Array[..result.Count], connectionId);
                }
            }
        }
    }
}
