using AutoFixture;
using FluentAssertions;
using HorrorArpg;
using HorrorArpg.Controllers;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Interfaces.Services;
using horrorarpg_backend.Tests.Controller_Tests.TestBase;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace horrorarpg_backend.Tests.Controller_Tests
{
    [TestFixture]
    public class SavesControllerTests : TestBase_ControllerTests
    {
        private SavesController _controller;
        private Mock<IUserSaveService> _userSaveServiceMock;

        [SetUp]
        public void Setup()
        {
            _userSaveServiceMock = new Mock<IUserSaveService>(MockBehavior.Loose);
            _controller = CreateController<SavesController>(_userSaveServiceMock.Object);
        }

        [Test]
        public async Task UpdateUserSave_ValidRequestWithAuth_ShouldReturnOkWithMessageAndData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupClaims(userId); // Mock claims for [Authorize]
            var dto = _fixture.Build<UserSaveDto>().Create();
            var response = _fixture.Build<UserSaveResponseDto>().Create();
            var expected = new { message = "User save updated", data = response };

            _userSaveServiceMock.Setup(s => s.UpdateUserSaveAsync(dto, userId)).ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUserSave(dto);

            // Assert
            AssertOk(result, expected);
            _userSaveServiceMock.Verify(s => s.UpdateUserSaveAsync(dto, userId), Times.Once);
        }

        [Test]
        public async Task UpdateUserSave_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("dto.SceneName", "Required"); // FIX: Add error to controller.ModelState (makes IsValid false, hits BadRequest before auth)

            // Act
            var result = await _controller.UpdateUserSave(new UserSaveDto());

            // Assert
            AssertBadRequestPlain(result, "Invalid data");
            _userSaveServiceMock.Verify(s => s.UpdateUserSaveAsync(It.IsAny<UserSaveDto>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task UpdateUserSave_NoAuthClaim_ShouldReturnUnauthorized()
        {
            // Arrange (default no claims)
            var dto = _fixture.Build<UserSaveDto>().Create();

            // Act
            var result = await _controller.UpdateUserSave(dto);

            // Assert
            AssertUnauthorizedObject(result, "Invalid token"); // FIX: Match UnauthorizedObjectResult with value
            _userSaveServiceMock.Verify(s => s.UpdateUserSaveAsync(It.IsAny<UserSaveDto>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task UpdateUserSave_InvalidTokenClaim_ShouldReturnUnauthorized()
        {
            // Arrange (invalid claim value - fails TryParse)
            SetupClaimsWithInvalid("invalid-guid");
            var dto = _fixture.Build<UserSaveDto>().Create();

            // Act
            var result = await _controller.UpdateUserSave(dto);

            // Assert
            AssertUnauthorizedObject(result, "Invalid token"); // FIX: Match UnauthorizedObjectResult with value
            _userSaveServiceMock.Verify(s => s.UpdateUserSaveAsync(It.IsAny<UserSaveDto>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task UpdateUserSave_ServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupClaims(userId);
            var dto = _fixture.Build<UserSaveDto>().Create();
            _userSaveServiceMock.Setup(s => s.UpdateUserSaveAsync(dto, userId)).ThrowsAsync(new Exception("Save conflict"));

            // Act
            var result = await _controller.UpdateUserSave(dto);

            // Assert
            AssertInternalServerError(result, "Save conflict");
            _userSaveServiceMock.Verify(s => s.UpdateUserSaveAsync(dto, userId), Times.Once);
        }

        [Test]
        public async Task GetUserSave_ValidUserId_ShouldReturnOkWithResponse()
        {
            // Arrange (anonymous allowed)
            var userId = Guid.NewGuid().ToString();
            var response = _fixture.Build<UserSaveResponseDto>().Create();
            _userSaveServiceMock.Setup(s => s.GetUserSaveAsync(It.IsAny<Guid>())).ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserSave(userId);

            // Assert
            AssertOk(result, response);
            _userSaveServiceMock.Verify(s => s.GetUserSaveAsync(Guid.Parse(userId)), Times.Once);
        }

        [Test]
        public async Task GetUserSave_InvalidUserId_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidUserId = "invalid-guid";

            // Act
            var result = await _controller.GetUserSave(invalidUserId);

            // Assert
            AssertBadRequestPlain(result, "Invalid userId");
            _userSaveServiceMock.Verify(s => s.GetUserSaveAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task GetUserSave_NoSave_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _userSaveServiceMock.Setup(s => s.GetUserSaveAsync(Guid.Parse(userId))).ReturnsAsync((UserSaveResponseDto)null);

            // Act
            var result = await _controller.GetUserSave(userId);

            // Assert
            AssertNotFound(result, "No user save");
            _userSaveServiceMock.Verify(s => s.GetUserSaveAsync(Guid.Parse(userId)), Times.Once);
        }
    }
}
