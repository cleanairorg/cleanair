using Application.Interfaces.Infrastructure.Websocket;
using Fleck;
using WebSocketBoilerplate;

namespace Api.Websocket;

public class Ping : BaseDto;

public class Pong : BaseDto;

public class PingEventHandler(IConnectionManager connectionManager)
    : BaseEventHandler<Ping>
{
    public override Task Handle(Ping dto, IWebSocketConnection socket)
    {
        var clientId = connectionManager.GetClientIdFromSocket(socket);
        socket.SendDto(new Pong());
        return Task.CompletedTask;
    }
}