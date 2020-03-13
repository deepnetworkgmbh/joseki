using System;
using System.Collections.Generic;

using joseki.db;

using Microsoft.EntityFrameworkCore;

namespace tests
{
    public static class JosekiTestsDb
    {
        public static JosekiDbContext CreateUniqueContext()
        {
            var options = new DbContextOptionsBuilder<JosekiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            return new JosekiDbContext(options);
        }
    }
}