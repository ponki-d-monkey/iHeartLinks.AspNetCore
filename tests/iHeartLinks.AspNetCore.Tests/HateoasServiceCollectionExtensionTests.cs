using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class HateoasServiceCollectionExtensionTests
    {
        private readonly Mock<IServiceCollection> mockSut;
        private readonly IServiceCollection sut;

        public HateoasServiceCollectionExtensionTests()
        {
            mockSut = new Mock<IServiceCollection>();
            mockSut
                .Setup(x => x.GetEnumerator())
                .Returns(Mock.Of<IEnumerator<ServiceDescriptor>>());

            sut = mockSut.Object;
        }

        [Fact]
        public void AddHateoasShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasShouldAddRequiredDependencies()
        {
            sut.AddHateoas();

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x => 
                x.Add(It.Is<ServiceDescriptor>(y => 
                    y.ServiceType == typeof(IActionContextAccessor) && 
                    y.Lifetime == ServiceLifetime.Singleton)), 
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);
        }

        [Fact]
        public void AddHateoasShouldOnlyAddOptionsWhenRequiredDependenciesAlreadyExist()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Singleton<IHttpContextAccessor, HttpContextAccessor>(),
                ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>(),
                ServiceDescriptor.Transient<IUrlHelperBuilder, UrlHelperBuilder>(),
                ServiceDescriptor.Transient<IHypermediaService, HypermediaService>()
            };

            mockSut
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.AddHateoas();

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas(o => { });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldThrowArgumentNullExceptionWhenConfigureOptionsDelegateIsNull()
        {
            Func<IServiceCollection> func = () => sut.AddHateoas(default(Action<HypermediaServiceOptions>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("configureOptions");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldAddRequiredDependencies()
        {
            sut.AddHateoas(o => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldOnlyAddOptionsWhenRequiredDependenciesAlreadyExist()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Singleton<IHttpContextAccessor, HttpContextAccessor>(),
                ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>(),
                ServiceDescriptor.Transient<IUrlHelperBuilder, UrlHelperBuilder>(),
                ServiceDescriptor.Transient<IHypermediaService, HypermediaService>()
            };

            mockSut
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.AddHateoas(o => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithHttpContextShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas((o, h) => { });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithHttpContextShouldThrowArgumentNullExceptionWhenConfigureOptionsDelegateIsNull()
        {
            Func<IServiceCollection> func = () => sut.AddHateoas(default(Action<HypermediaServiceOptions, IHttpContextAccessor>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("configureOptions");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithHttpContextShouldAddRequiredDependencies()
        {
            sut.AddHateoas((o, h) => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);
        }

        [Fact]
        public void AddHateoasWithHttpContextShouldOnlyAddOptionsWhenRequiredDependenciesAlreadyExist()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Singleton<IHttpContextAccessor, HttpContextAccessor>(),
                ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>(),
                ServiceDescriptor.Transient<IUrlHelperBuilder, UrlHelperBuilder>(),
                ServiceDescriptor.Transient<IHypermediaService, HypermediaService>()
            };

            mockSut
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.AddHateoas((o, h) => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);
        }
    }
}
