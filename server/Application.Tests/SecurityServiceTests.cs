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
        
    }
}