using AutoFixture;
using FluentAssertions;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace horrorarpg_backend.Tests.Infrastructure_Unit_Tests
{
    [TestFixture]
    public class UserSaveRepositoryTests : TestBase_UserRepositoryTests
    {
        private UserSaveRepository _repository;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            // Prevent circular reference exception
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
            _repository = new UserSaveRepository(_context);
        }

        [Test]
        public async Task CreateAsync_ShouldAddUserSave()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                          .Without(u => u.UserSave)
                          .Create();
            await _context.Users.AddAsync(user);

            var userSave = _fixture.Build<UserSaveEntity>()
                .With(us => us.UserId, user.UserId)
                .Without(us => us.User)
                .Create();

            // Act
            var result = await _repository.CreateAsync(userSave);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.UserId);

            var dbCheck = await _context.UserSaves.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            dbCheck.Should().NotBeNull();
            dbCheck.SceneName.Should().Be(userSave.SceneName);
        }

        [Test]
        public async Task GetByUserIdAsync_ShouldReturnUserSave_WithEagerLoadedUser()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                           .Without(u => u.UserSave)
                           .Create();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userSave = _fixture.Build<UserSaveEntity>()
            .Without(us => us.User)
            .With(us => us.UserId, user.UserId)
            .Create();

            await _context.UserSaves.AddAsync(userSave);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(user.UserId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.UserId);
            result.User.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateAsync_ShouldModifyExistingUserSave()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                          .Without(u => u.UserSave)
                          .Create();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userSave = _fixture.Build<UserSaveEntity>()
                .With(us => us.UserId, user.UserId)
                .Without(us => us.User)
                .With(us => us.SceneName, "OldScene")
                .Create();

            await _context.UserSaves.AddAsync(userSave);
            await _context.SaveChangesAsync();

            // Modify something
            userSave.SceneName = "UpdatedScene";

            // Act
            await _repository.UpdateAsync(userSave);

            // Assert
            var updated = await _context.UserSaves.FirstAsync(us => us.UserId == user.UserId);
            updated.SceneName.Should().Be("UpdatedScene");
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveUserSave()
        {
            // Arrange
            var user = _fixture.Build<UserEntity>()
                          .Without(u => u.UserSave)
                          .Create();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userSave = _fixture.Build<UserSaveEntity>()
                .With(us => us.UserId, user.UserId)
                .Without(us => us.User)
                .Create();

            await _context.UserSaves.AddAsync(userSave);
            await _context.SaveChangesAsync();

            // Act
            var deleted = await _repository.DeleteAsync(user.UserId);

            // Assert
            deleted.Should().BeTrue();
            (await _context.UserSaves.AnyAsync(us => us.UserId == user.UserId)).Should().BeFalse();
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnFalse_WhenUserSaveDoesNotExist()
        {
            // Act
            var deleted = await _repository.DeleteAsync(Guid.NewGuid());

            // Assert
            deleted.Should().BeFalse();
        }
    }
}
