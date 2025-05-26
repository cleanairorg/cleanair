using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Api.Rest.ControllerTests;

[TestFixture]
public class AuthControllerTests
{
    private Mock<ISecurityService> _securityServiceMock = null!;
    private Mock<ILoggingService> _loggerMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _securityServiceMock = new Mock<ISecurityService>();
        _loggerMock = new Mock<ILoggingService>();

        _controller = new AuthController(
            _securityServiceMock.Object,
            _loggerMock.Object
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }


    [Test]
    public void Login_ShouldReturnAuthResponse()
    {
        var dto = new AuthLoginRequestDto { Email = "user@test.com", Password = "pass" };
        var expected = new AuthResponseDto { Jwt = "abc123" };

        _securityServiceMock.Setup(s => s.Login(dto)).Returns(expected);

        var result = _controller.Login(dto);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(expected));
    }


    [Test]
    public void Register_ShouldReturnAuthResponse()
    {
        var dto = new AuthRegisterRequestDto { Email = "user@test.com", Password = "pass" };
        var expected = new AuthResponseDto { Jwt = "xyz456" };

        _securityServiceMock.Setup(s => s.Register(dto)).Returns(expected);

        var result = _controller.Register(dto);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(expected));
    }

    [Test]
    public void GetUserInfo_ShouldReturnUserInfo()
    {
        var email = "user@test.com";
        var expected = new AuthGetUserInfoDto { Email = email, Role = "user" };

        _securityServiceMock.Setup(s => s.GetUserInfo(email)).Returns(expected);

        var result = _controller.GetUserInfo(email);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(expected));
    }

    [Test]
    public void Secured_ValidJwt_ShouldReturnOk()
    {
        var jwt = "valid.jwt.token";
        _controller.HttpContext.Request.Headers["Authorization"] = $"Bearer {jwt}";

        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow(jwt));

        var result = _controller.Secured();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo("You are authorized to see this message"));
    }
}
