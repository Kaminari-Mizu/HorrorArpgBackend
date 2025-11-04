using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using horrorarpg_backend.Infrastructure;
using horrorarpg_backend.Infrastructure.Repositories;
using NUnit.Framework;

namespace horrorarpg_backend.Tests.Infrastructure_Unit_Tests
{
    public abstract class TestBase_UserRepositoryTests
    {
        protected ArpgDbContext _context;
        protected UserRepository _userRepository;

        [SetUp]
        public void BaseSetup()
        {
            // Use unique in-memory DB per test to prevent data bleed
            var options = new DbContextOptionsBuilder<ArpgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ArpgDbContext(options);
            _context.Database.EnsureCreated();

            _userRepository = new UserRepository(_context);
        }

        [TearDown]
        public void BaseTearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}