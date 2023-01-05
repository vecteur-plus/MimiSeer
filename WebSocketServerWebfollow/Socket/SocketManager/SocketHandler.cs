using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServerWebfollow.SocketManager
{
    public abstract class SocketHandler
    {
        public ConnectionManager Connections { get; set; }

        public SocketHandler(ConnectionManager connections_)
        {
            Connections = connections_;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            await Task.Run(() => { Connections.AddSocket(socket); });
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await Connections.RemoveSocketAsync(Connections.GetId(socket));
        }

        public virtual async Task SendMessage(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open) return;

            await socket.SendAsync(new System.ArraySegment<byte>(Encoding.Default.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
                );
        }

        public async Task SendMessage(string id, string message)
        {
            await SendMessage(Connections.GetSocketById(id), message);
        }

        public async Task SendMessageToAll(string message)
        {
            foreach (var conn in Connections.GetAllConnections())
            {
                await SendMessage(conn.Value.WebSocket, message);
            }
        }

        public abstract Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}