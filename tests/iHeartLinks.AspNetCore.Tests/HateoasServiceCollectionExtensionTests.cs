using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using iHeartLinks.Core;
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
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
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
                    y.ImplementationType == typeof(ActionContextAccessor) && 
                    y.Lifetime == ServiceLifetime.Singleton)), 
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);
        }

        [Fact]
        public void AddHateoasShouldOnlyAddOptionsWhenRequiredDependenciesAlreadyExist()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>(),
                ServiceDescriptor.Scoped<IUrlHelperBuilder, UrlHelperBuilder>(),
                ServiceDescriptor.Scoped<IHypermediaService, HypermediaService>()
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
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas(o => { });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");
        }

        [Fact]
        public void AddHateoasWithBuilderOptionsShouldThrowArgumentNullExceptionWhenConfigureOptionsDelegateIsNull()
        {
            Func<IServiceCollection> func = () => sut.AddHateoas(default(Action<HypermediaServiceOptions>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("configureOptions");
        }

        [Fact]
        public void AddHateoasWithBuilderOptionsShouldAddRequiredDependencies()
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
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);
        }

        [Fact]
        public void AddHateoasWithOptionsShouldOnlyAddOptionsWhenRequiredDependenciesAlreadyExist()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>(),
                ServiceDescriptor.Scoped<IUrlHelperBuilder, UrlHelperBuilder>(),
                ServiceDescriptor.Scoped<IHypermediaService, HypermediaService>()
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
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Never);
        }

        [Fact]
        public void AddHateoasWithBuilderOptionsShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas((o, b) => { });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");
        }

        [Fact]
        public void AddHateoasWithOptionsShouldThrowArgumentNullExceptionWhenConfigureOptionsDelegateIsNull()
        {
            Func<IServiceCollection> func = () => sut.AddHateoas(default(Action<HypermediaServiceOptions, IUrlHelperBuilder>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("configureOptions");
        }

        [Fact]
        public void AddHateoasWithOptionsShouldAddRequiredDependencies()
        {
            sut.AddHateoas((o, b) => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IConfigureOptions<HypermediaServiceOptions>) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IHypermediaService) &&
                    y.ImplementationType == typeof(HypermediaService) &&
                    y.Lifetime == ServiceLifetime.Scoped)),
                Times.Once);
        }
    }
}
