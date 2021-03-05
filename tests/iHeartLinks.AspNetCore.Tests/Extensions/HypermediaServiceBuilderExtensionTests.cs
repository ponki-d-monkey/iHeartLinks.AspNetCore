using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlProviders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class HypermediaServiceBuilderExtensionTests
    {
        private readonly Mock<IServiceCollection> mockServices;

        private readonly HypermediaServiceBuilder sut;

        public HypermediaServiceBuilderExtensionTests()
        {
            mockServices = new Mock<IServiceCollection>();
            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(Mock.Of<IEnumerator<ServiceDescriptor>>());

            sut = new HypermediaServiceBuilder(mockServices.Object);
        }

        [Fact]
        public void UseExtendedLinkShouldThrowArgumentNullExceptionWhenHypermediaServiceBuilderIsNull()
        {
            Func<HypermediaServiceBuilder> func = () => default(HypermediaServiceBuilder).UseExtendedLink();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Fact]
        public void UseExtendedLinkShouldAddRequiredDependencies()
        {
            sut.UseExtendedLink();

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IQueryNameSelector) &&
                   y.ImplementationType == typeof(QueryNameSelector) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IUrlProvider) &&
                   y.ImplementationType == typeof(WithTemplatedUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   y.ImplementationType == typeof(IsTemplatedEnricher) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   y.ImplementationType == typeof(HttpMethodEnricher) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkFactory))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   y.ImplementationType == typeof(HttpLinkFactory) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseExtendedLinkShouldReplaceLinkFactoryWithHttpLinkFactoryWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<ILinkFactory, LinkFactory>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseExtendedLink();

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   y.ImplementationType == typeof(HttpLinkFactory) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseExtendedLinkShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseExtendedLink();

            result.Should().BeSameAs(sut);
        }
    }
}
