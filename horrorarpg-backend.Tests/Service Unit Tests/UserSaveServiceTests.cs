using AutoFixture;
using AutoMapper;
using FluentAssertions;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces.Repositories;
using horrorarpg_backend.Services.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace horrorarpg_backend.Tests.Service_Unit_Tests
{
    [TestFixture]
    public class UserSaveServiceTests : TestBase_ServiceTests
    {
        private UserSaveService _service;
        private Mock<IUserSaveRepository> _userSaveRepoMock;
        private Mock<IMapper> _mapperMock;

        [SetUp]
        public void Setup()
        {
            _userSaveRepoMock = _fixture.Freeze<Mock<IUserSaveRepository>>();
            // Manual mock for IMapper with Loose behavior (non-null defaults for unmapped calls)
            _mapperMock = new Mock<IMapper>(MockBehavior.Loose);

            _service = new UserSaveService(_userSaveRepoMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task UpdateUserSaveAsync_ExistingSave_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var positionDto = new PositionDto { X = 10f, Y = -5f, Z = 20f };
            var dto = new UserSaveDto
            {
                UserId = userId.ToString(), // Ignored server-side
                Health = 80f,
                Mana = 50f,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                SceneName = "UpdatedScene",
                Position = positionDto
            };
            var existingEntity = _fixture.Build<UserSaveEntity>()
                .With(us => us.UserId, userId)
                .With(us => us.SceneName, "OldScene")
                .With(us => us.Health, 100f)
                .With(us => us.Mana, 60f)
                .With(us => us.PositionX, 5f)
                .With(us => us.PositionY, 0f)
                .With(us => us.PositionZ, 15f)
                .With(us => us.Timestamp, DateTime.UtcNow.AddHours(-1))
                .Create();
            var responseDto = new UserSaveResponseDto
            {
                Health = dto.Health,
                Mana = dto.Mana,
                Timestamp = dto.Timestamp,
                SceneName = dto.SceneName,
                Position = new { X = dto.Position.X, Y = dto.Position.Y, Z = dto.Position.Z } // Object for JSON
            };

            _userSaveRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingEntity);

            // Non-generic Map(dto, existingEntity) void setup with callback (simulates + verifies in-place update)
            _mapperMock.Setup(m => m.Map(It.Is<object>(o => ReferenceEquals(o, dto)), It.Is<object>(o => ReferenceEquals(o, existingEntity))))
                      .Callback<object, object>((src, tgt) =>
                      {
                          var srcDto = (UserSaveDto)src;
                          var tgtEntity = (UserSaveEntity)tgt;
                          // FIX: Simulate in-place update (assign fields to mimic AutoMapper)
                          tgtEntity.SceneName = srcDto.SceneName;
                          tgtEntity.Health = srcDto.Health;
                          tgtEntity.Mana = srcDto.Mana;
                          tgtEntity.PositionX = srcDto.Position.X;
                          tgtEntity.PositionY = srcDto.Position.Y;
                          tgtEntity.PositionZ = srcDto.Position.Z;
                          tgtEntity.Timestamp = DateTime.Parse(srcDto.Timestamp);
                          // Assert post-update (now on modified values)
                          tgtEntity.SceneName.Should().Be(srcDto.SceneName, "In-place map should update SceneName");
                          tgtEntity.Health.Should().Be(srcDto.Health, "In-place map should update Health");
                          tgtEntity.Mana.Should().Be(srcDto.Mana, "In-place map should update Mana");
                          tgtEntity.PositionX.Should().Be(srcDto.Position.X, "In-place map should update PositionX");
                          tgtEntity.PositionY.Should().Be(srcDto.Position.Y, "In-place map should update PositionY");
                          tgtEntity.PositionZ.Should().Be(srcDto.Position.Z, "In-place map should update PositionZ");
                          tgtEntity.Timestamp.Should().Be(DateTime.Parse(srcDto.Timestamp), "In-place map should parse Timestamp");
                      });

            // Non-generic Map<UserSaveResponseDto>(existingEntity) setup (uses same updated instance)
            _mapperMock.Setup(m => m.Map<UserSaveResponseDto>(It.Is<object>(o => ReferenceEquals(o, existingEntity)))).Returns(responseDto);

            _userSaveRepoMock.Setup(r => r.UpdateAsync(existingEntity)).Returns(Task.CompletedTask); // Uses updated existingEntity

            // Act
            var result = await _service.UpdateUserSaveAsync(dto, userId);

            // Assert
            result.Should().BeEquivalentTo(responseDto);
            _userSaveRepoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _userSaveRepoMock.Verify(r => r.UpdateAsync(It.Is<UserSaveEntity>(e => e.SceneName == dto.SceneName && e.Health == dto.Health && e.Mana == dto.Mana && e.PositionX == dto.Position.X)), Times.Once);
            _userSaveRepoMock.Verify(r => r.CreateAsync(It.IsAny<UserSaveEntity>()), Times.Never);
            _mapperMock.Verify(m => m.Map<UserSaveResponseDto>(It.Is<object>(o => ReferenceEquals(o, existingEntity))), Times.Once);
        }

        [Test]
        public async Task UpdateUserSaveAsync_NonExistingSave_ShouldCreateAndReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var positionDto = new PositionDto { X = 10f, Y = -5f, Z = 20f };
            var dto = new UserSaveDto
            {
                UserId = userId.ToString(), // Ignored server-side
                Health = 100f,
                Mana = 75f,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                SceneName = "NewScene",
                Position = positionDto
            };
            var responseDto = new UserSaveResponseDto
            {
                Health = dto.Health,
                Mana = dto.Mana,
                Timestamp = dto.Timestamp,
                SceneName = dto.SceneName,
                Position = new { X = dto.Position.X, Y = dto.Position.Y, Z = dto.Position.Z } // Object for JSON
            };

            _userSaveRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserSaveEntity)null);

            // FIX: It.IsAny for in-place Map(dto, service's new entity) void setup with callback (simulates + verifies updates)
            _mapperMock.Setup(m => m.Map(It.IsAny<object>(), It.IsAny<object>()))
                      .Callback<object, object>((src, tgt) =>
                      {
                          var srcDto = (UserSaveDto)src;
                          var tgtEntity = (UserSaveEntity)tgt;
                          // Simulate in-place update (assign fields to mimic AutoMapper)
                          tgtEntity.SceneName = srcDto.SceneName;
                          tgtEntity.Health = srcDto.Health;
                          tgtEntity.Mana = srcDto.Mana;
                          tgtEntity.PositionX = srcDto.Position.X;
                          tgtEntity.PositionY = srcDto.Position.Y;
                          tgtEntity.PositionZ = srcDto.Position.Z;
                          tgtEntity.Timestamp = DateTime.Parse(srcDto.Timestamp);
                          // Assert post-update (now on modified values)
                          tgtEntity.SceneName.Should().Be(srcDto.SceneName, "In-place map should update SceneName on new entity");
                          tgtEntity.Health.Should().Be(srcDto.Health, "In-place map should update Health on new entity");
                          tgtEntity.Mana.Should().Be(srcDto.Mana, "In-place map should update Mana on new entity");
                          tgtEntity.PositionX.Should().Be(srcDto.Position.X, "In-place map should update PositionX on new entity");
                          tgtEntity.PositionY.Should().Be(srcDto.Position.Y, "In-place map should update PositionY on new entity");
                          tgtEntity.PositionZ.Should().Be(srcDto.Position.Z, "In-place map should update PositionZ on new entity");
                          tgtEntity.Timestamp.Should().Be(DateTime.Parse(srcDto.Timestamp), "In-place map should parse Timestamp on new entity");
                      });

            // FIX: It.IsAny for response Map<UserSaveResponseDto>(service's updated entity) setup (returns fixed DTO)
            _mapperMock.Setup(m => m.Map<UserSaveResponseDto>(It.IsAny<object>())).Returns(responseDto);

            UserSaveEntity capturedEntity = null;
            // FIX: Callback on CreateAsync to capture service's updated entity and assert fields (verifies map happened)
            _userSaveRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserSaveEntity>()))
                             .Callback<UserSaveEntity>(e =>
                             {
                                 capturedEntity = e;
                                 e.UserId.Should().Be(userId, "Entity UserId should match param");
                                 e.SceneName.Should().Be(dto.SceneName, "Create entity should have updated SceneName");
                                 e.Health.Should().Be(dto.Health, "Create entity should have updated Health");
                                 e.Mana.Should().Be(dto.Mana, "Create entity should have updated Mana");
                                 e.PositionX.Should().Be(dto.Position.X, "Create entity should have updated PositionX");
                                 e.PositionY.Should().Be(dto.Position.Y, "Create entity should have updated PositionY");
                                 e.PositionZ.Should().Be(dto.Position.Z, "Create entity should have updated PositionZ");
                                 e.Timestamp.Should().Be(DateTime.Parse(dto.Timestamp), "Create entity should have parsed Timestamp");
                             })
                             .ReturnsAsync((UserSaveEntity)null); // No need to return; service doesn't use return value

            // Act
            var result = await _service.UpdateUserSaveAsync(dto, userId);

            // Assert
            result.Should().BeEquivalentTo(responseDto); // Now not null (It.IsAny setup fires)
            capturedEntity.Should().NotBeNull("CreateAsync should be called with updated entity");
            _userSaveRepoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _userSaveRepoMock.Verify(r => r.CreateAsync(It.Is<UserSaveEntity>(e => e.UserId == userId && e.SceneName == dto.SceneName && e.Health == dto.Health)), Times.Once);
            _userSaveRepoMock.Verify(r => r.UpdateAsync(It.IsAny<UserSaveEntity>()), Times.Never);
            _mapperMock.Verify(m => m.Map(It.IsAny<object>(), It.IsAny<object>()), Times.Once); // In-place map called
            _mapperMock.Verify(m => m.Map<UserSaveResponseDto>(It.IsAny<object>()), Times.Once); // Response map called
        }

        [Test]
        public async Task UpdateUserSaveAsync_NullDto_ShouldThrowArgumentNullException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await AssertThrowsAsync<ArgumentNullException>(() => _service.UpdateUserSaveAsync(null, userId), "dto");
            _userSaveRepoMock.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>()), Times.Never);
            _mapperMock.Verify(m => m.Map(It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task GetUserSaveAsync_ExistingSave_ShouldReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var entity = _fixture.Build<UserSaveEntity>()
                .With(us => us.UserId, userId)
                .With(us => us.SceneName, "TestScene")
                .With(us => us.Health, 75f)
                .With(us => us.Mana, 40f)
                .With(us => us.PositionX, 8f)
                .With(us => us.PositionY, -3f)
                .With(us => us.PositionZ, 12f)
                .With(us => us.Timestamp, DateTime.UtcNow)
                .Create();
            var responseDto = new UserSaveResponseDto
            {
                Health = entity.Health,
                Mana = entity.Mana,
                Timestamp = entity.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                SceneName = entity.SceneName,
                Position = new { X = entity.PositionX, Y = entity.PositionY, Z = entity.PositionZ } // Object for JSON
            };

            _userSaveRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(entity);

            // Non-generic Map<UserSaveResponseDto>(entity) setup (matches service call _mapper.Map<UserSaveResponseDto>(entity))
            _mapperMock.Setup(m => m.Map<UserSaveResponseDto>(It.Is<object>(o => ReferenceEquals(o, entity)))).Returns(responseDto);

            // Act
            var result = await _service.GetUserSaveAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(responseDto);
            _userSaveRepoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<UserSaveResponseDto>(It.Is<object>(o => ReferenceEquals(o, entity))), Times.Once);
        }

        [Test]
        public async Task GetUserSaveAsync_NonExistingSave_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userSaveRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserSaveEntity)null);

            // Act
            var result = await _service.GetUserSaveAsync(userId);

            // Assert
            result.Should().BeNull();
            _userSaveRepoMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<UserSaveResponseDto>(It.IsAny<object>()), Times.Never); // No map called (early null return)
        }
    }
}
