
namespace Server.Interfaces
{
    public interface ICustomWebSocketMiddleware
    {
        Task Invoke(HttpContent context);

    }
}
