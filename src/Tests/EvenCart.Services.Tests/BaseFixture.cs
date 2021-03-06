﻿using System;
using EvenCart.Core.Caching;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Infrastructure.Providers;
using EvenCart.Data.Database;
using EvenCart.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EvenCart.Services.Tests
{

    public abstract class BaseFixture
    {
       
        protected BaseFixture()
        {
            var serviceCollection = new ServiceCollection();
            //mock the hosting env
            var hostingEnvironment = new Mock<IHostingEnvironment>();

            hostingEnvironment.Setup(x => x.ApplicationName)
                .Returns("Hosting:UnitTestEnvironment");

            hostingEnvironment.Setup(x => x.EnvironmentName)
                .Returns(ApplicationConfig.TestEnvironmentName);

            hostingEnvironment.Setup(x => x.ContentRootPath)
                .Returns(AppDomain.CurrentDomain.BaseDirectory);

            hostingEnvironment.Setup(x => x.ContentRootFileProvider)
                .Returns(new LocalFileProvider(hostingEnvironment.Object));

            //mock httpcontext
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

            var configuration = new TestConfiguration();
            serviceCollection.AddSingleton<IHostingEnvironment>(provider => hostingEnvironment.Object);
            serviceCollection.AddSingleton<IHttpContextAccessor>(provider => httpContextAccessor.Object);
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IDatabaseSettings>(new TestDbInit.TestDatabaseSettings());
            ApplicationEngine.ConfigureServices(serviceCollection, hostingEnvironment.Object, configuration);
            //set the cache providers
            CacheProviders.PrimaryProvider =
                DependencyResolver.Resolve<ICacheProvider>(typeof(MemoryCacheProvider).FullName);
        }
    }
}