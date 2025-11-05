using FluentAssertions;
using AutoFixture;
using HorrorArpg;
using HorrorArpg.Controllers;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Interfaces.Services;
using horrorarpg_backend.Tests.Controller_Tests.TestBase;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace horrorarpg_backend.Tests.Controller_Tests
{
    [TestFixture]
    public class AuthControllerTests : TestBase_ControllerTests
    {
        private AuthController _controller;
        private Mock<IUserService> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>(MockBehavior.Loose);
            _controller = CreateController<AuthController>(_userServiceMock.Object);
        }

        [Test]
        public async Task Login_ValidRequest_ShouldReturnOkWithResponse()
        {
            // Arrange
            var request = _fixture.Build<LoginRequestDto>().Create(); // FIX: Build.Create for precise overload resolution
            var response = _fixture.Build<LoginResponseDto>().Create(); // { Token, User: UserDto with string UserId for frontend }

            _userServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            AssertOk(result, response);
            _userServiceMock.Verify(s => s.LoginAsync(request), Times.Once);
        }

        [Test]
        public async Task Login_InvalidRequest_ShouldReturnBadRequestWithError()
        {
            // Arrange
            var request = _fixture.Build<LoginRequestDto>().Create(); // FIX: Build.Create
            _userServiceMock.Setup(s => s.LoginAsync(request)).ThrowsAsync(new Exception("Invalid credentials"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            AssertBadRequest(result, "Invalid credentials"); // { Error } for #ff6b6b notifications in LoginForm.tsx
            _userServiceMock.Verify(s => s.LoginAsync(request), Times.Once);
        }

        [Test]
        public async Task Register_ValidRequest_ShouldReturnOkWithResponse()
        {
            // Arrange
            var request = _fixture.Build<LoginRequestDto>().Create(); // FIX: Build.Create
            var response = _fixture.Build<RegisterResponseDto>().Create(); // { Message, User: UserDto }

            _userServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.Register(request);

            // Assert
            AssertOk(result, response);
            _userServiceMock.Verify(s => s.RegisterAsync(request), Times.Once);
        }

        [Test]
        public async Task Register_ExistingUser_ShouldReturnBadRequestWithError()
        {
            // Arrange
            var request = _fixture.Build<LoginRequestDto>().Create(); // FIX: Build.Create
            _userServiceMock.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new Exception("User already exists"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            AssertBadRequest(result, "User already exists");
            _userServiceMock.Verify(s => s.RegisterAsync(request), Times.Once);
        }

        [Test]
        public async Task Register_GeneralException_ShouldReturnBadRequestWithError()
        {
            // Arrange
            var request = _fixture.Build<LoginRequestDto>().Create(); // FIX: Build.Create
            _userServiceMock.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            AssertBadRequest(result, "Database error");
            _userServiceMock.Verify(s => s.RegisterAsync(request), Times.Once);
        }
    }
}
