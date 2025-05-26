using System.Text.Json;
using Application.Interfaces.Infrastructure.Websocket;
using Fleck;
using Infrastructure.Websocket;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Websocket.Tests;

[TestFixture]
public class DictionaryConnectionManagerTests
{
    private WebSocketConnectionManager _manager;
    private Mock<ILogger<WebSocketConnectionManager>> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<WebSocketConnectionManager>>();
        _manager = new WebSocketConnectionManager(_logger.Object);
    }

    private static Mock<IWebSocketConnection> BuildMockSocket(string id, bool isAvailable = true)
    {
        var socket = new Mock<IWebSocketConnection>();
        var info = new Mock<IWebSocketConnectionInfo>();
        info.Setup(i => i.Id).Returns(Guid.Parse(id));

        socket.Setup(s => s.ConnectionInfo).Returns(info.Object);
        socket.Setup(s => s.IsAvailable).Returns(isAvailable);
        socket.Setup(s => s.Send(It.IsAny<string>())).Returns(Task.CompletedTask);

        return socket;
    }

    [Test]
    public async Task OnOpen_ShouldRegisterClient()
    {
        var clientId = "client-1";
        var socketId = Guid.NewGuid().ToString();
        var socketMock = BuildMockSocket(socketId);

        await _manager.OnOpen(socketMock.Object, clientId);

        var storedSocket = _manager.GetSocketFromClientId(clientId);
        var resolvedClientId = _manager.GetClientIdFromSocket(socketMock.Object);

        Assert.AreEqual(clientId, resolvedClientId);
        Assert.AreEqual(socketMock.Object, storedSocket);
    }

    [Test]
    public async Task OnClose_ShouldRemoveConnection()
    {
        var clientId = "client-2";
        var socketId = Guid.NewGuid().ToString();
        var socket = BuildMockSocket(socketId);

        await _manager.OnOpen(socket.Object, clientId);
        await _manager.OnClose(socket.Object, clientId);

        Assert.Throws<Exception>(() => _manager.GetSocketFromClientId(clientId));
        Assert.Throws<Exception>(() => _manager.GetClientIdFromSocket(socket.Object));
    }

    [Test]
    public async Task AddToTopic_ShouldTrackTopicMembership()
    {
        var clientId = "client-3";
        var topic = "updates";

        await _manager.AddToTopic(topic, clientId);

        var topics = await _manager.GetTopicsFromMemberId(clientId);
        var members = await _manager.GetMembersFromTopicId(topic);

        Assert.Contains(topic, topics);
        Assert.Contains(clientId, members);
    }

    [Test]
    public async Task RemoveFromTopic_ShouldRemoveCleanly()
    {
        var clientId = "client-4";
        var topic = "notifications";

        await _manager.AddToTopic(topic, clientId);
        await _manager.RemoveFromTopic(topic, clientId);

        var topics = await _manager.GetTopicsFromMemberId(clientId);
        var members = await _manager.GetMembersFromTopicId(topic);

        Assert.IsFalse(topics.Contains(topic));
        Assert.IsFalse(members.Contains(clientId));
    }

    [Test]
    public async Task BroadcastToTopic_ShouldSendIfClientOnline()
    {
        var clientId = "client-5";
        var topic = "broadcast";
        var socketId = Guid.NewGuid().ToString();
        var socket = BuildMockSocket(socketId);

        await _manager.OnOpen(socket.Object, clientId);
        await _manager.AddToTopic(topic, clientId);

        var message = new { Msg = "Hello" };
        await _manager.BroadcastToTopic(topic, message);

        socket.Verify(s => s.Send(It.Is<string>(json => json.Contains("Hello"))), Times.Once);
    }

    [Test]
    public void GetClientIdFromSocket_ThrowsIfNotFound()
    {
        var socket = BuildMockSocket(Guid.NewGuid().ToString());

        Assert.Throws<Exception>(() => _manager.GetClientIdFromSocket(socket.Object));
    }

    [Test]
    public void GetSocketFromClientId_ThrowsIfNotFound()
    {
        Assert.Throws<Exception>(() => _manager.GetSocketFromClientId("ghost"));
    }
}
