using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace horrorarpg_backend.Tests.Service_Unit_Tests
{
    public abstract class TestBase_ServiceTests
    {
        protected Fixture _fixture;
        [SetUp]
        public void BaseSetup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize(new AutoMoqCustomization()); // Enables auto-mocking for deps
        }

        // Helper: Creates a basic config mock with JWT secret
        protected Mock<IConfiguration> CreateConfigMock(string jwtSecret = "test-jwt-secret-32-chars-long-for-hmac")
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Jwt:Secret"]).Returns(jwtSecret);
            return configMock;
        }

        // Helper: Mocks Mapper.Map for simple entity ↔ DTO roundtrips (lenient with It.IsAny)
        protected Mock<IMapper> CreateMapperMock<TSource, TDest>(TSource source, TDest dest)
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TSource, TDest>(It.IsAny<TSource>())) // FIX: It.IsAny for robustness
                      .Returns(dest);
            mapperMock.Setup(m => m.Map<TDest, TSource>(It.IsAny<TDest>()))
                      .Returns(source);
            mapperMock.Setup(m => m.Map<TSource, TSource>(It.IsAny<TSource>())) // For update scenarios
                      .Returns<TSource>(s => s); // Identity for same-type updates
                                                 // For Map(sourceDto, existingEntity) in updates (void, updates in-place)
            mapperMock.Setup(m => m.Map(It.IsAny<TSource>(), It.IsAny<TDest>()))
                      .Returns((TSource src, TDest tgt) => { /* Simulate update */ return tgt; });
            return mapperMock;
        }

        // Helper: Asserts async task throws specific exception
        protected async Task AssertThrowsAsync<TException>(Func<Task> act, string because = "")
            where TException : Exception
        {
            var ex = await act.Should().ThrowAsync<TException>().Where(e => e.Message.Contains(because)).ConfigureAwait(false);
            ex.And.Message.Should().Contain(because);
        }
    }
}
