using AutoFixture;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace horrorarpg_backend.Tests.Infrastructure_Unit_Tests
{
    [TestFixture]
    public class UserRepositoryTests : TestBase_UserRepositoryTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            // Prevent recursive property issues with AutoFixture
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task CreateAsync_ValidUser_AddsUserToDatabase()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                               .With(u => u.UserId, Guid.NewGuid())
                               .With(u => u.Username, "TestUser")
                               .Create();

            // Act
            var result = await _userRepository.CreateAsync(user);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(user.UserId));
            Assert.That(_context.Users.Any(u => u.UserId == user.UserId), Is.True);
        }

        [Test]
        public async Task GetByUserIdAsync_ExistingUser_ReturnsCorrectUser()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                               .With(u => u.UserId, Guid.NewGuid())
                               .With(u => u.Username, "UserById")
                               .Create();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByUserIdAsync(user.UserId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(user.UserId));
            Assert.That(result.Username, Is.EqualTo("UserById"));
        }

        [Test]
        public async Task GetByUserIdAsync_NonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _userRepository.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUserNameAsync_ExistingUser_ReturnsCorrectUser()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                               .With(u => u.UserId, Guid.NewGuid())
                               .With(u => u.Username, "FindMe")
                               .Create();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByUserNameAsync("FindMe");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("FindMe"));
        }

        [Test]
        public async Task GetByUserNameAsync_NonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _userRepository.GetByUserNameAsync("DoesNotExist");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ThenRetrieveWithGetByUserIdAsync_ReturnsSameUser()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                               .With(u => u.UserId, Guid.NewGuid())
                               .With(u => u.Username, "IntegrationCheck")
                               .Create();

            await _userRepository.CreateAsync(user);

            // Act
            var retrieved = await _userRepository.GetByUserIdAsync(user.UserId);

            // Assert
            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.UserId, Is.EqualTo(user.UserId));
            Assert.That(retrieved.Username, Is.EqualTo("IntegrationCheck"));
        }
    }
}
