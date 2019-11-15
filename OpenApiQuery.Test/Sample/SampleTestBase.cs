using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Sample;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    public class SampleTestBase
    {
        protected TestServer SetupSample(
            IEnumerable<User> testdata = null,
            Action<WebHostBuilder> setup = null
        )
        {
            var databaseName = "Blog_" + Guid.NewGuid();
            var builder = new WebHostBuilder();
            builder.UseStartup<Startup>();
            builder.ConfigureServices(s =>
            {
                s.AddDbContext<BlogDbContext>(o => o.UseInMemoryDatabase(databaseName));
            });
            setup?.Invoke(builder);

            var server = new TestServer(builder);
            if (testdata != null)
            {
                using (var scope = server.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                    context.Database.EnsureCreated();
                    context.Users.AddRange(testdata);
                    context.SaveChanges();
                }
            }

            return server;
        }
    }
}