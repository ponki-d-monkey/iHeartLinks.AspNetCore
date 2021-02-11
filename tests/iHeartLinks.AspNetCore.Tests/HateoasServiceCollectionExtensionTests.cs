using System;
using FluentAssertions;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
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
                    y.ImplementationType == typeof(ActionContextAccessor) &&
                    y.Lifetime == ServiceLifetime.Singleton)),
                Times.Never);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelper) &&
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
                    y.ServiceType == typeof(IActionContextAccessor) && 
                    y.ImplementationType == typeof(ActionContextAccessor) && 
                    y.Lifetime == ServiceLifetime.Singleton)), 
                Times.Once);

            mockSut.Verify(x =>
                x.Add(It.Is<ServiceDescriptor>(y =>
                    y.ServiceType == typeof(IUrlHelper) &&
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
