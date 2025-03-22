using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OnlineExamSystem.Domains;

namespace OnlineExamSystem.Tests.Helpers
{
    public static class TestDbContext
    {
        public static ApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Suppress transaction warning
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}