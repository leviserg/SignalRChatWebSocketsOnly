using System.Net.WebSockets;

namespace Server.Interfaces
{
    public interface IConnectionManager
    {
        string AddSocket(WebSocket socket);
        Task RemoveSocket(string id);
        Task ProcessMessage(byte[] messageBytes, string sender);

    }
}
