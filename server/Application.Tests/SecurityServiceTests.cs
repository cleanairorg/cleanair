using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Services;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Application.Models.Enums;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

namespace Application.Tests.Services
{
    [TestFixture]
    public class SecurityServiceTests
    {
        private SecurityService _securityService;
        private Mock<IOptionsMonitor<AppOptions>> _optionsMonitorMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private AppOptions _appOptions;

        [SetUp]
        public void Setup()
        {
            _appOptions = new AppOptions
            {
                JwtSecret = "TestSecret1234567890TestSecret1234567890"
            };

            _optionsMonitorMock = new Mock<IOptionsMonitor<AppOptions>>();
            _optionsMonitorMock.Setup(m => m.CurrentValue).Returns(_appOptions);

            _userRepositoryMock = new Mock<IUserRepository>();

            _securityService = new SecurityService(_optionsMonitorMock.Object, _userRepositoryMock.Object);
        }

        [Test]
        public void HashPassword_ShouldReturnConsistentHash()
        {
            // Arrange
            var password = "MySecurePassword123";
            
            // Act
            var hash1 = _securityService.HashPassword(password);
            var hash2 = _securityService.HashPassword(password);
            
            // Assert
            hash1.Should().Be(hash2);
            hash1.Should().NotBeEmpty();
        }
        
        [Test]
        public void VerifyPasswordOrThrow_CorrectPassword_ShouldNotThrow()
        {
            // Arrange
            var password = "secret123";
            var salt = _securityService.GenerateSalt();
            var hash = _securityService.HashPassword(password + salt);

            // Act & Assert
            Assert.DoesNotThrow(() => _securityService.VerifyPasswordOrThrow(password + salt, hash));
        }

        [Test]
        public void VerifyPasswordOrThrow_IncorrectPassword_ShouldThrowAuthenticationException()
        {
            // Arrange
            var salt = _securityService.GenerateSalt();
            var correctHash = _securityService.HashPassword("correctpassword" + salt);

            // Act & Assert
            var ex = Assert.Throws<AuthenticationException>(() =>
                _securityService.VerifyPasswordOrThrow("wrongpassword" + salt, correctHash));
            Assert.That(ex.Message, Is.EqualTo("Invalid login"));
        }
        
        [Test]
        public void GenerateSalt_ShouldReturnUniqueValues()
        {
            // Act
            var salt1 = _securityService.GenerateSalt();
            var salt2 = _securityService.GenerateSalt();

            // Assert
            salt1.Should().NotBe(salt2);
            salt1.Should().NotBeNullOrEmpty();
            salt2.Should().NotBeNullOrEmpty();
        }
        
        
        [Test]
        public void Register_UserAlreadyExists_ShouldThrowValidationException()
        {
            // Arrange
            var dto = new AuthRegisterRequestDto { Email = "existing@test.com", Password = "pass123", Role = "user" };
            _userRepositoryMock.Setup(x => x.GetUserOrNull(dto.Email)).Returns(new User());

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _securityService.Register(dto));
            Assert.That(ex.Message, Is.EqualTo("User already exists"));
        }

        [Test]
        public void Register_ValidNewUser_ShouldReturnJwt()
        {
            // Arrange
            var dto = new AuthRegisterRequestDto { Email = "new@test.com", Password = "pass123", Role = "admin" };

            _userRepositoryMock.Setup(x => x.GetUserOrNull(dto.Email)).Returns((User)null!);
            _userRepositoryMock.Setup(x => x.AddUser(It.IsAny<User>()))
                .Returns((User u) => u);

            // Act
            var result = _securityService.Register(dto);

            // Assert
            result.Jwt.Should().NotBeNullOrEmpty();
        }
        
        [Test]
        public void Login_InvalidEmail_ShouldThrowValidationException()
        {
            // Arrange
            var dto = new AuthLoginRequestDto { Email = "nonexistent@test.com", Password = "pass" };
            _userRepositoryMock.Setup(r => r.GetUserOrNull(dto.Email)).Returns((User)null!);

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _securityService.Login(dto));
            Assert.That(ex.Message, Is.EqualTo("Username not found"));
        }

        [Test]
        public void Login_InvalidPassword_ShouldThrowAuthenticationException()
        {
            // Arrange
            var salt = _securityService.GenerateSalt();
            var correctHash = _securityService.HashPassword("correctpassword" + salt);
            var dto = new AuthLoginRequestDto { Email = "user@test.com", Password = "wrongpassword" };

            var user = new User { Email = dto.Email, Hash = correctHash, Salt = salt, Id = "123", Role = "user" };
            _userRepositoryMock.Setup(r => r.GetUserOrNull(dto.Email)).Returns(user);

            // Act & Assert
            var ex = Assert.Throws<AuthenticationException>(() => _securityService.Login(dto));
            Assert.That(ex.Message, Is.EqualTo("Invalid login"));
        }

        [Test]
        public void Login_ValidCredentials_ShouldReturnJwt()
        {
            // Arrange
            var password = "pass123";
            var salt = _securityService.GenerateSalt();
            var hash = _securityService.HashPassword(password + salt);

            var dto = new AuthLoginRequestDto { Email = "test@test.com", Password = password };

            var user = new User { Id = "abc123", Email = dto.Email, Role = "user", Salt = salt, Hash = hash };
            _userRepositoryMock.Setup(r => r.GetUserOrNull(dto.Email)).Returns(user);

            // Act
            var result = _securityService.Login(dto);

            // Assert
            result.Jwt.Should().NotBeNullOrEmpty();
        }

        
        [Test]
        public void GetUserInfo_UserNotFound_ShouldThrowValidationException()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetUserOrNull("notfound@test.com")).Returns((User)null!);

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _securityService.GetUserInfo("notfound@test.com"));
            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public void GetUserInfo_ExistingUser_ShouldReturnUserInfo()
        {
            // Arrange
            var user = new User { Id = "u1", Email = "user@test.com", Role = "user" };
            _userRepositoryMock.Setup(r => r.GetUserOrNull(user.Email)).Returns(user);

            // Act
            var result = _securityService.GetUserInfo(user.Email);

            // Assert
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Role.Should().Be(user.Role);
        }
        
        [Test]
        public void VerifyJwtOrThrow_ValidToken_ReturnsExpectedClaims()
        {
            // Arrange
            var expectedClaims = new JwtClaims
            {
                Id = "abc123",
                Role = "admin",
                Exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(),
                Email = "jwt@test.com"
            };

            var jwt = new JwtBuilder()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .WithSecret(_appOptions.JwtSecret)
                .WithUrlEncoder(new JwtBase64UrlEncoder())
                .WithJsonSerializer(new JsonNetSerializer())
                .AddClaim(nameof(JwtClaims.Id), expectedClaims.Id)
                .AddClaim(nameof(JwtClaims.Role), expectedClaims.Role)
                .AddClaim(nameof(JwtClaims.Exp), expectedClaims.Exp)
                .AddClaim(nameof(JwtClaims.Email), expectedClaims.Email)
                .Encode();

            // Act
            var result = _securityService.VerifyJwtOrThrow(jwt);

            // Assert
            result.Id.Should().Be(expectedClaims.Id);
            result.Role.Should().Be(expectedClaims.Role);
            result.Email.Should().Be(expectedClaims.Email);
            result.Exp.Should().Be(expectedClaims.Exp);
        }

        [Test]
        public void VerifyJwtOrThrow_InvalidToken_ShouldThrowException()
        {
            // Arrange
            var invalidJwt = "this.is.not.a.valid.token";

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _securityService.VerifyJwtOrThrow(invalidJwt));
            ex.Message.Should().Contain("Token must consist of 3 delimited by dot parts.");
        }
        

        [Test]
        public void AdminRole_ShouldBeAdmin()
        {
            Assert.That(Constants.AdminRole, Is.EqualTo("admin"));
        }

        [Test]
        public void UserRole_ShouldBeUser()
        {
            Assert.That(Constants.UserRole, Is.EqualTo("user"));
        }
    }
    
}