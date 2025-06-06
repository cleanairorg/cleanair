﻿using Api.Rest.Controllers;
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
    private Mock<IWebsocketSubscriptionService> _subscriptionServiceMock = null!;
    private SubscriptionController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _securityServiceMock = new Mock<ISecurityService>();
        _subscriptionServiceMock = new Mock<IWebsocketSubscriptionService>();

        _controller = new SubscriptionController(
            _securityServiceMock.Object,
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

        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow("token")); // Setup (optional if no return)

        var result = await _controller.Subscribe("token", dto);

        Assert.That(result, Is.InstanceOf<OkResult>());

        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow("token"), Times.Once);  // <-- verify called
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

        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow("auth"), Times.Once);  // <-- verify called
        _subscriptionServiceMock.Verify(s => s.UnsubscribeFromTopic("client123", dto.TopicIds), Times.Once);
    }
} 
