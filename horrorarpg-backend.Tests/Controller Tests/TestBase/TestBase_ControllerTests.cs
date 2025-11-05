using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace horrorarpg_backend.Tests.Controller_Tests.TestBase
{
    public abstract class TestBase_ControllerTests
    {
        protected Fixture _fixture;
        protected Mock<HttpContext> _httpContextMock;
        protected ClaimsPrincipal _claimsPrincipal;
        protected ModelStateDictionary _modelState; // FIX: Real instance (add errors in test Arrange)

        [SetUp]
        public void BaseSetup_Controller()
        {
            // Fixture setup (standalone - no service inheritance)
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _httpContextMock = new Mock<HttpContext>();
            // FIX: Default to empty ClaimsPrincipal (no claims for unauthorized tests)
            _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            _httpContextMock.Setup(c => c.User).Returns(_claimsPrincipal);
            _modelState = new ModelStateDictionary(); // FIX: Real ModelState (add errors in Arrange; IsValid computed from errors)
        }

        // Helper: Creates controller with injected service and mocked HttpContext
        protected TController CreateController<TController>(object service) where TController : ControllerBase
        {
            var controller = (TController)Activator.CreateInstance(typeof(TController), service);
            controller.ControllerContext = new ControllerContext { HttpContext = _httpContextMock.Object };
            // FIX: No ModelState assignment (read-only); add errors in test Arrange if needed
            return controller;
        }

        // Helper: Sets ModelState validity (use in Arrange for invalid cases)
        protected void SetModelStateValid(bool isValid)
        {
            _modelState.Clear(); // Clear previous errors
            if (!isValid)
            {
                _modelState.AddModelError("dto", "Invalid data"); // Example error for binding failures
            }
        }

        // Helper: Sets claims for [Authorize] tests
        protected void SetupClaims(Guid userId)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
            _claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(_claimsPrincipal);
        }

        // Helper: Asserts Ok(200) with expected value
        protected void AssertOk<T>(IActionResult result, T expected)
        {
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expected);
        }

        // Helper: Asserts BadRequest(400) with { Error = msg } (for exception cases)
        protected void AssertBadRequest(IActionResult result, string errorMsg)
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = (BadRequestObjectResult)result;
            badResult.StatusCode.Should().Be(400);
            badResult.Value.Should().BeEquivalentTo(new { Error = errorMsg });
        }

        // Helper: Asserts BadRequest(400) with plain message (for validation cases)
        protected void AssertBadRequestPlain(IActionResult result, string message)
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = (BadRequestObjectResult)result;
            badResult.StatusCode.Should().Be(400);
            badResult.Value.Should().Be(message);
        }

        // Helper: Asserts Unauthorized(401)
        protected void AssertUnauthorized(IActionResult result)
        {
            result.Should().BeOfType<UnauthorizedResult>();
            var unauthResult = (UnauthorizedResult)result;
            unauthResult.StatusCode.Should().Be(401);
        }

        // Helper: Asserts NotFound(404) with msg
        protected void AssertNotFound(IActionResult result, string msg)
        {
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)result;
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be(msg);
        }

        // Helper: Asserts StatusCode(500) with { Error = msg }
        protected void AssertInternalServerError(IActionResult result, string errorMsg)
        {
            result.Should().BeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.Should().Be(500);
            errorResult.Value.Should().BeEquivalentTo(new { Error = errorMsg });
        }
    }
}
