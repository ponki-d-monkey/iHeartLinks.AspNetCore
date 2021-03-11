using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
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
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
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
        public void AddHateoasShouldAddDefaultDependencies()
        {
            sut.AddHateoas();

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
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
        public void AddHateoasShouldReturnSameInstanceOfServiceCollection()
        {
            var result = sut.AddHateoas();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddHateoasWithHypemerdiaServiceBuilderShouldThrowArgumentNullExceptionWhenServiceCollectionIsNull()
        {
            Func<IServiceCollection> func = () => default(IServiceCollection).AddHateoas(b => { });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
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
        public void AddHateoasWithHypemerdiaServiceBuilderShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IServiceCollection> func = () => sut.AddHateoas(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Never);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
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
        public void AddHateoasWithHypemerdiaServiceBuilderShouldAddDefaultDependencies()
        {
            sut.AddHateoas(b => { });

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
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
        public void AddHateoasWithHypermediaServiceBuilderShouldInvokeBuilder()
        {
            // It's not important which services are added by the builder in this unit test since there is a dedicated 
            // unit test for HypermediaServiceBuilder. The important thing is to know that the builder is being invoked.
            sut.AddHateoas(b => b
                .UseRelativeUrlHref()
                .UseExtendedLink());

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelperBuilder) &&
                    y.ImplementationType == typeof(UrlHelperBuilder) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(EmptyBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(NonTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlPathProvider) &&
                    y.ImplementationType == typeof(WithTemplatedUrlPathProvider) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   y.ImplementationType == typeof(IsTemplatedEnricher) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   y.ImplementationType == typeof(HttpMethodEnricher) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(LinkFactory) &&
                    y.Lifetime == ServiceLifetime.Transient)),
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(ILinkFactory) &&
                    y.ImplementationType == typeof(HttpLinkFactory) &&
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
        public void AddHateoasWithHypermediaServiceBuilderShouldReturnSameInstanceOfServiceCollection()
        {
            var result = sut.AddHateoas(b => { });

            result.Should().BeSameAs(sut);
        }
    }
}
