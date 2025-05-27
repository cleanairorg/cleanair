using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace Api.Rest.ControllerTests;

[TestFixture]
public class SubscriptionControllerTests
{
    private Mock<ISecurityService> _securityServiceMock = null!;
    private Mock<IConnectionManager> _connectionManagerMock = null!;
    private Mock<IWebsocketSubscriptionService> _subscriptionServiceMock = null!;
    private SubscriptionController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _securityServiceMock = new Mock<ISecurityService>();
        _connectionManagerMock = new Mock<IConnectionManager>();
        _subscriptionServiceMock = new Mock<IWebsocketSubscriptionService>();

        _controller = new SubscriptionController(
            _securityServiceMock.Object,
            _connectionManagerMock.Object,
            _subscriptionServiceMock.Object);
    }

    [Test]
    public async Task Subscribe_ValidRequest_ShouldReturnOk()
    {
        var dto = new ChangeSubscriptionDto
        {
            ClientId = "client123",
            TopicIds = new List<string> { "topic1", "topic2" }
        };

        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow("token"));

        var result = await _controller.Subscribe("token", dto);

        Assert.That(result, Is.InstanceOf<OkResult>());
        _subscriptionServiceMock.Verify(s => s.SubscribeToTopic("client123", dto.TopicIds), Times.Once);
    }

    [Test]
    public async Task Unsubscribe_ValidRequest_ShouldReturnOk()
    {
        var dto = new ChangeSubscriptionDto
        {
            ClientId = "client123",
            TopicIds = new List<string> { "topicA" }
        };

        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow("auth"));

        var result = await _controller.Unsubscribe("auth", dto);

        Assert.That(result, Is.InstanceOf<OkResult>());
        _subscriptionServiceMock.Verify(s => s.UnsubscribeFromTopic("client123", dto.TopicIds), Times.Once);
    }
} 
