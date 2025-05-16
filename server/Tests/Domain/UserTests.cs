using Core.Domain.Entities;
using FluentAssertions;

namespace Tests.Domain
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void User_WithValidProperties_ShouldHaveCorrectValues()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "test@example.com";
            
            // Act
            var user = new User
            {
                Id = userId,
                Email = email,
                Hash = "hashedpassword",
                Salt = "salt",
                Role = "User"
            };

            // Assert
            user.Id.Should().Be(userId);
            user.Email.Should().Be(email);
            user.Role.Should().Be("User");
        }
    }
}