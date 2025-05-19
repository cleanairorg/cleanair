using Core.Domain.Entities;
using FluentAssertions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Tests
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private MyDbContext _context;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            // Opsæt in-memory database
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new MyDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetUserOrNull_WithExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = "user123",
                Email = email,
                Hash = "hash",
                Salt = "salt",
                Role = "User"
            };
            
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _userRepository.GetUserOrNull(email);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Id.Should().Be("user123");
        }

        [Test]
        public void GetUserOrNull_WithNonExistingEmail_ShouldReturnNull()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@example.com";

            // Act
            var result = _userRepository.GetUserOrNull(nonExistentEmail);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void AddUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = "user456",
                Email = "new@example.com",
                Hash = "hash",
                Salt = "salt",
                Role = "User"
            };

            // Act
            var result = _userRepository.AddUser(user);

            // Assert
            result.Should().Be(user);
            
            // Verify the user is in the database
            var savedUser = _context.Users.Find(user.Id);
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be(user.Email);
        }
    }
}