using System.Collections.Concurrent;
using System.Text.Json;
using Application.Interfaces.Infrastructure.Websocket;
using Fleck;
using Infrastructure.Websocket;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
    
    // Mutant Testing
    
    [Test]
    public async Task AddToTopic_ShouldLogCurrentState()
    {
        // Arrange
        var clientId = "client-8";
        var topic = "mutant-topic";

        var logCalled = false;
        _logger.Setup(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Current state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )).Callback(() => logCalled = true);

        // Act
        await _manager.AddToTopic(topic, clientId);

        // Assert
        Assert.IsTrue(logCalled, "Expected LogCurrentState to be invoked and log the current state");
    }
    
    [Test]
public async Task RemoveFromTopic_ShouldRemoveTopic_WhenLastMemberRemoved()
{
    // Arrange
    var topic = "mutant-news";
    var member = "client-9";

    await _manager.AddToTopic(topic, member);

    // Act
    await _manager.RemoveFromTopic(topic, member);

    // Assert
    var topics = await _manager.GetTopicsFromMemberId(member);
    var members = await _manager.GetMembersFromTopicId(topic);

    Assert.IsEmpty(topics, "Expected member to have no topics");
    Assert.IsEmpty(members, "Expected topic to have no members");
    
    var internalTopicsField = typeof(WebSocketConnectionManager)
        .GetField("_topicMembers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
    var internalMap = internalTopicsField?.GetValue(_manager) as ConcurrentDictionary<string, HashSet<string>>;
    Assert.IsFalse(internalMap!.ContainsKey(topic), "Expected topic to be fully removed from internal state");
}

[Test]
public async Task RemoveFromTopic_ShouldNotRemoveTopic_WhenOtherMembersExist()
{
    // Arrange
    var topic = "shared-topic";
    var member1 = "client-10";
    var member2 = "client-11";

    await _manager.AddToTopic(topic, member1);
    await _manager.AddToTopic(topic, member2);

    // Act
    await _manager.RemoveFromTopic(topic, member1);

    // Assert
    var remainingMembers = await _manager.GetMembersFromTopicId(topic);

    Assert.IsTrue(remainingMembers.Contains(member2), "Expected other member to remain");
    Assert.IsFalse(remainingMembers.Contains(member1), "Expected removed member to be gone");

    var internalTopicsField = typeof(WebSocketConnectionManager)
        .GetField("_topicMembers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
    var internalMap = internalTopicsField?.GetValue(_manager) as ConcurrentDictionary<string, HashSet<string>>;
    Assert.IsTrue(internalMap!.ContainsKey(topic), "Expected topic to still exist");
}

[Test]
public async Task RemoveFromTopic_ShouldRemoveMemberFromTopicList_WhenLastTopicRemoved()
{
    // Arrange
    var topic = "weather";
    var member = "client-12";

    await _manager.AddToTopic(topic, member);

    // Act
    await _manager.RemoveFromTopic(topic, member);

    // Assert
    var memberTopics = await _manager.GetTopicsFromMemberId(member);
    Assert.IsEmpty(memberTopics, "Expected member to have no topics");

    var internalField = typeof(WebSocketConnectionManager)
        .GetField("_memberTopics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    var map = internalField?.GetValue(_manager) as ConcurrentDictionary<string, HashSet<string>>;
    Assert.IsFalse(map!.ContainsKey(member), "Expected member to be removed from internal map");
}

[Test]
public async Task RemoveFromTopic_ShouldNotRemoveMember_WhenTheyHaveOtherTopics()
{
    // Arrange
    var member = "client-13";
    var topic1 = "sensors";
    var topic2 = "humidity";

    await _manager.AddToTopic(topic1, member);
    await _manager.AddToTopic(topic2, member);

    // Act
    await _manager.RemoveFromTopic(topic1, member);

    // Assert
    var remainingTopics = await _manager.GetTopicsFromMemberId(member);

    Assert.IsFalse(remainingTopics.Contains(topic1), "Expected removed topic to be gone");
    Assert.IsTrue(remainingTopics.Contains(topic2), "Expected second topic to remain");

    var internalField = typeof(WebSocketConnectionManager)
        .GetField("_memberTopics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    var map = internalField?.GetValue(_manager) as ConcurrentDictionary<string, HashSet<string>>;
    Assert.IsTrue(map!.ContainsKey(member), "Expected member to still exist in the map");
}

[Test]
public async Task RemoveFromTopic_ShouldLogCurrentState()
{
    // Arrange
    var clientId = "client-14";
    var topic = "dust";

    // Add then remove to trigger LogCurrentState
    await _manager.AddToTopic(topic, clientId);

    var logInvoked = false;
    _logger.Setup(
        x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Current state")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        )).Callback(() => logInvoked = true);

    // Act
    await _manager.RemoveFromTopic(topic, clientId);

    // Assert
    Assert.IsTrue(logInvoked, "Expected LogCurrentState to be invoked via logging after removal");
}

[Test]
public async Task BroadcastToTopic_ShouldSendCamelCaseJsonAndLogDebug()
{
    // Arrange
    var topic = "sensor-alerts";
    var clientId = "client-15";
    var socketId = Guid.NewGuid().ToString();
    var message = new { AlertLevel = "High", SensorId = 42 };

    var socketMock = new Mock<IWebSocketConnection>();
    var connectionInfoMock = new Mock<IWebSocketConnectionInfo>();

    connectionInfoMock.Setup(x => x.Id).Returns(Guid.Parse(socketId));
    socketMock.Setup(x => x.ConnectionInfo).Returns(connectionInfoMock.Object);
    socketMock.Setup(x => x.IsAvailable).Returns(true);

    string? capturedJson = null;
    socketMock.Setup(s => s.Send(It.IsAny<string>()))
        .Callback<string>(json => capturedJson = json)
        .Returns(Task.CompletedTask);

    var logCalled = false;
    _logger.Setup(
        x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) =>
                v.ToString()!.Contains("Sent message to client") && v.ToString()!.Contains(clientId)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        )).Callback(() => logCalled = true);

    // Inject the socket and topic
    await _manager.OnOpen(socketMock.Object, clientId);
    await _manager.AddToTopic(topic, clientId);

    // Act
    await _manager.BroadcastToTopic(topic, message);

    // Assert camelCase JSON (kills mutant #1)
    Assert.IsNotNull(capturedJson, "Expected a JSON message to be sent.");
    StringAssert.Contains("alertLevel", capturedJson);
    StringAssert.Contains("sensorId", capturedJson);
    StringAssert.DoesNotContain("AlertLevel", capturedJson);
    StringAssert.DoesNotContain("SensorId", capturedJson);

    // Assert debug log happened
    Assert.IsTrue(logCalled, "Expected LogDebug to be called after sending message.");
}

[Test]
public void GetSocketFromClientId_ShouldThrow_WithDetailedMessage_IfClientNotFound()
{
    // Arrange
    var missingClientId = "ghost-client";

    // Act
    var ex = Assert.Throws<Exception>(() => _manager.GetSocketFromClientId(missingClientId));

    // Assert
    StringAssert.Contains("Could not find socket for clientId", ex!.Message);
    StringAssert.Contains(missingClientId, ex.Message);
}


}
