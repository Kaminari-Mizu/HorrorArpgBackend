using AutoFixture;
using AutoMapper;
using FluentAssertions;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces.Repositories;
using horrorarpg_backend.Services.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace horrorarpg_backend.Tests.Service_Unit_Tests
{
    [TestFixture]
    public class UserServiceTests : TestBase_ServiceTests
    {
        private UserService _service;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IConfiguration> _configMock;

        [SetUp]
        public void Setup()
        {
            _userRepoMock = _fixture.Freeze<Mock<IUserRepository>>();
            // Manual mock for IMapper with Loose behavior (non-null defaults for unmapped generics)
            _mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            _configMock = CreateConfigMock();

            _service = new UserService(_userRepoMock.Object, _mapperMock.Object, _configMock.Object);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ShouldReturnTokenAndUserDto()
        {
            // Arrange
            var request = _fixture.Create<LoginRequestDto>();
            var userEntity = _fixture.Build<UserEntity>()
                .With(u => u.UserId, Guid.NewGuid())
                .With(u => u.Username, request.Username)
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(request.Password))
                .Create();
            // Build userDto from userEntity (Guid UserId, UserName casing)
            var userDto = new UserDto
            {
                UserId = userEntity.UserId, // Guid-to-Guid
                UserName = userEntity.Username // Align casing
            };

            _userRepoMock.Setup(r => r.GetByUserNameAsync(request.Username)).ReturnsAsync(userEntity);

            // FIX: Non-generic Map<TDest>(object source) setup (matches service call _mapper.Map<UserDto>(userEntity))
            _mapperMock.Setup(m => m.Map<UserDto>(It.Is<object>(o => ReferenceEquals(o, userEntity)))).Returns(userDto);

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Should().BeEquivalentTo(userDto); // Now matches (non-generic setup fires)
            result.Token.Should().StartWith("eyJ"); // Basic JWT prefix check (real gen)
            _userRepoMock.Verify(r => r.GetByUserNameAsync(request.Username), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(It.Is<object>(o => ReferenceEquals(o, userEntity))), Times.Once); // Exact verify
        }

        [Test]
        public async Task LoginAsync_InvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var request = _fixture.Create<LoginRequestDto>();
            var userEntity = _fixture.Build<UserEntity>()
                .With(u => u.Username, request.Username)
                .With(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword("wrong-password"))
                .Create();

            _userRepoMock.Setup(r => r.GetByUserNameAsync(request.Username)).ReturnsAsync(userEntity);

            // No mapper setup needed (exception before map)

            // Act & Assert
            await AssertThrowsAsync<Exception>(() => _service.LoginAsync(request), "Invalid credentials");
            _userRepoMock.Verify(r => r.GetByUserNameAsync(request.Username), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<object>()), Times.Never); // No map called (early exception)
        }

        [Test]
        public async Task LoginAsync_NonExistentUser_ShouldThrowException()
        {
            // Arrange
            var request = _fixture.Create<LoginRequestDto>();
            _userRepoMock.Setup(r => r.GetByUserNameAsync(request.Username)).ReturnsAsync((UserEntity)null);

            // No mapper setup needed (exception before repo/map)

            // Act & Assert
            await AssertThrowsAsync<Exception>(() => _service.LoginAsync(request), "Invalid credentials");
            _userRepoMock.Verify(r => r.GetByUserNameAsync(request.Username), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<object>()), Times.Never); // No map called
        }

        [Test]
        public async Task RegisterAsync_ValidRequest_ShouldCreateUserAndReturnResponse()
        {
            // Arrange
            var request = _fixture.Create<LoginRequestDto>();
            var mappedEntity = _fixture.Build<UserEntity>()
                .With(u => u.Username, request.Username) // From mapper (pre-service overrides)
                .Create(); // No PasswordHash/CreatedAt yet (set in service)
            var userEntity = mappedEntity; // Alias; service sets UserId = Guid.NewGuid()
            var userDto = new UserDto
            {
                UserId = Guid.NewGuid(), // Service sets new Guid
                UserName = request.Username // Align casing
            };

            _userRepoMock.Setup(r => r.GetByUserNameAsync(request.Username)).ReturnsAsync((UserEntity)null);

            // FIX: Non-generic Map<TDest>(object source) setups (matches service calls _mapper.Map<UserEntity>(request) and _mapper.Map<UserDto>(user))
            _mapperMock.Setup(m => m.Map<UserEntity>(It.Is<object>(o => ReferenceEquals(o, request)))).Returns(mappedEntity); // Initial map: Sets Username
            _mapperMock.Setup(m => m.Map<UserDto>(It.Is<object>(o => ReferenceEquals(o, userEntity)))).Returns(userDto); // Response map: Post-service user

            bool wasCalled = false;
            string capturedPasswordHash = null;

            // Setup with Callback (It.IsAny for leniency, as arg is service-modified userEntity)
            _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserEntity>()))
                         .Callback<UserEntity>(u =>
                         {
                             wasCalled = true;
                             capturedPasswordHash = u.PasswordHash;
                             u.Username.Should().Be(request.Username, "Username should match request"); // Now passes (from non-generic mapper setup)
                             BCrypt.Net.BCrypt.Verify(request.Password, u.PasswordHash).Should().BeTrue("Password should hash correctly");
                         })
                         .ReturnsAsync(userEntity);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Registration successful");
            result.User.Should().BeEquivalentTo(userDto);

            _userRepoMock.Verify(r => r.GetByUserNameAsync(request.Username), Times.Once);
            _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<UserEntity>()), Times.Once);
            _mapperMock.Verify(m => m.Map<UserEntity>(It.Is<object>(o => ReferenceEquals(o, request))), Times.Once); // Exact verify
            _mapperMock.Verify(m => m.Map<UserDto>(It.Is<object>(o => ReferenceEquals(o, userEntity))), Times.Once); // Exact verify

            wasCalled.Should().BeTrue("CreateAsync should be called");
            capturedPasswordHash.Should().NotBeNullOrEmpty("PasswordHash should be set");
        }

        [Test]
        public async Task RegisterAsync_ExistingUser_ShouldThrowException()
        {
            // Arrange
            var request = _fixture.Create<LoginRequestDto>();
            var existingUser = _fixture.Create<UserEntity>();

            _userRepoMock.Setup(r => r.GetByUserNameAsync(request.Username)).ReturnsAsync(existingUser);

            // No mapper/repo create setup needed (exception before map/create)

            // Act & Assert
            await AssertThrowsAsync<Exception>(() => _service.RegisterAsync(request), "User already exists");
            _userRepoMock.Verify(r => r.GetByUserNameAsync(request.Username), Times.Once);
            _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<UserEntity>()), Times.Never);
            _mapperMock.Verify(m => m.Map<UserEntity>(It.IsAny<object>()), Times.Never); // No map called (early exception)
        }
    }
}
