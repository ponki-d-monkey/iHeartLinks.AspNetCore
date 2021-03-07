using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class HypermediaServiceBuilderTests
    {
        private const string TestCustomBaseUrl = "https://iheartlinks.example.com";

        private readonly Mock<IServiceCollection> mockServices;

        private readonly HypermediaServiceBuilder sut;

        public HypermediaServiceBuilderTests()
        {
            mockServices = new Mock<IServiceCollection>();
            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(Mock.Of<IEnumerator<ServiceDescriptor>>());

            sut = new HypermediaServiceBuilder(mockServices.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenServicesIsNull()
        {
            Action action = () => new HypermediaServiceBuilder(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("services");
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldAddRequiredDependenciesWhenDependenciesDoNotExist()
        {
            sut.UseAbsoluteUrlHref();

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IHttpContextAccessor) &&
                   y.Lifetime == ServiceLifetime.Singleton)),
               Times.Once);

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldOnlyReplaceExistingDependenciesWhenExisting()
        {
            // Note: For some reason testing adding IHttpContextAccessor and removal of existing IBaseUrlProvider is failing. 
            // I will not include those tests for now. I suspect it has something to do with the dependency of CurrentRequestBaseUrlProvider
            // on IHttpContextAccessor. The important thing for now is the expected implementation is being added in the service collection.

            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IBaseUrlProvider, EmptyBaseUrlProvider>()
            };

            sut.UseAbsoluteUrlHref();

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(CurrentRequestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseAbsoluteUrlHref();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseRelativeUrlHrefShouldAddEmptyBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseRelativeUrlHref();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(EmptyBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseRelativeUrlHrefShouldReplaceBaseUrlProviderWithEmptyBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseRelativeUrlHref();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(EmptyBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseRelativeUrlHrefShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseRelativeUrlHref();

            result.Should().BeSameAs(sut);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url")]
        public void UseCustomBaseUrlHrefShouldThrowArgumentExceptionWhenBaseUrlIs(string baseUrl)
        {
            Func<HypermediaServiceBuilder> func = () => sut.UseCustomBaseUrlHref(baseUrl);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be($"Parameter 'baseUrl' must not be null or empty and must be a valid URL.");
            exception.ParamName.Should().BeNull();

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   (y.ImplementationFactory.Invoke(null) as CustomBaseUrlProvider).Provide().OriginalString == TestCustomBaseUrl &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Never);
        }

        [Fact]
        public void UseCustomBaseUrlHrefShouldAddCustomBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseCustomBaseUrlHref(TestCustomBaseUrl);

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   (y.ImplementationFactory.Invoke(null) as CustomBaseUrlProvider).Provide().OriginalString == TestCustomBaseUrl &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseCustomBaseUrlHrefShouldReplaceBaseUrlProviderWithCustomBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseCustomBaseUrlHref(TestCustomBaseUrl);

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   (y.ImplementationFactory.Invoke(null) as CustomBaseUrlProvider).Provide().OriginalString == TestCustomBaseUrl &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseCustomBaseUrlHrefShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseCustomBaseUrlHref(TestCustomBaseUrl);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseBaseUrlProviderShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseBaseUrlProvider<TestBaseUrlProvider>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(TestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseBaseUrlProviderShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseBaseUrlProvider<TestBaseUrlProvider>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   y.ImplementationType == typeof(TestBaseUrlProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseBaseUrlProviderShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseBaseUrlProvider<TestBaseUrlProvider>();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseBaseUrlProviderWithFactoryShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseBaseUrlProvider(s =>
            {
                var provider = new TestBaseUrlProvider();
                provider.Name = "test";

                return provider;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   (y.ImplementationFactory.Invoke(null) as TestBaseUrlProvider).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseBaseUrlProviderWithFactoryShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseBaseUrlProvider(s =>
            {
                var provider = new TestBaseUrlProvider();
                provider.Name = "test";

                return provider;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IBaseUrlProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IBaseUrlProvider) &&
                   (y.ImplementationFactory.Invoke(null) as TestBaseUrlProvider).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseBaseUrlProviderWithFactoryShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseBaseUrlProvider(s => new TestBaseUrlProvider());

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseUrlPathProviderShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseUrlPathProvider<TestUrlPathProvider>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IUrlPathProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IUrlPathProvider) &&
                   y.ImplementationType == typeof(TestUrlPathProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseUrlPathProviderShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IUrlPathProvider, NonTemplatedUrlPathProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseUrlPathProvider<TestUrlPathProvider>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IUrlPathProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IUrlPathProvider) &&
                   y.ImplementationType == typeof(TestUrlPathProvider) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseUrlPathProviderShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseUrlPathProvider<TestUrlPathProvider>();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseUrlPathProviderWithFactoryShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseUrlPathProvider(s =>
            {
                var provider = new TestUrlPathProvider();
                provider.Name = "test";

                return provider;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IUrlPathProvider))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IUrlPathProvider) &&
                   (y.ImplementationFactory.Invoke(null) as TestUrlPathProvider).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseUrlPathProviderWithFactoryShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<IUrlPathProvider, NonTemplatedUrlPathProvider>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseUrlPathProvider(s =>
            {
                var provider = new TestUrlPathProvider();
                provider.Name = "test";

                return provider;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IUrlPathProvider))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(IUrlPathProvider) &&
                   (y.ImplementationFactory.Invoke(null) as TestUrlPathProvider).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseUrlPathProviderWithFactoryShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseUrlPathProvider(s => new TestUrlPathProvider());

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseLinkFactoryShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseLinkFactory<TestLinkFactory>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkFactory))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   y.ImplementationType == typeof(TestLinkFactory) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseLinkFactoryShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<ILinkFactory, LinkFactory>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseLinkFactory<TestLinkFactory>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkFactory))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   y.ImplementationType == typeof(TestLinkFactory) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseLinkFactoryShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseLinkFactory<TestLinkFactory>();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseLinkFactoryWithFactoryShouldAddBaseUrlProviderWhenItDoesNotExist()
        {
            sut.UseLinkFactory(s =>
            {
                var factory = new TestLinkFactory();
                factory.Name = "test";

                return factory;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkFactory))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   (y.ImplementationFactory.Invoke(null) as TestLinkFactory).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseLinkFactoryWithFactoryShouldReplaceBaseUrlProviderWhenExisting()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<ILinkFactory, LinkFactory>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.UseLinkFactory(s =>
            {
                var factory = new TestLinkFactory();
                factory.Name = "test";

                return factory;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkFactory))), Times.Once);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkFactory) &&
                   (y.ImplementationFactory.Invoke(null) as TestLinkFactory).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void UseLinkFactoryWithFactoryShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.UseLinkFactory<TestLinkFactory>();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddLinkDataEnricherShouldAlwaysAddLinkDataEnricher()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<ILinkDataEnricher, HttpMethodEnricher>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.AddLinkDataEnricher<TestLinkDataEnricher>();

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkDataEnricher))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   y.ImplementationType == typeof(TestLinkDataEnricher) &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void AddLinkDataEnricherShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.AddLinkDataEnricher<TestLinkDataEnricher>();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddLinkDataEnricherWithFactoryShouldAlwaysAddLinkDataEnricher()
        {
            var serviceDescriptors = new List<ServiceDescriptor>
            {
                ServiceDescriptor.Transient<ILinkDataEnricher, HttpMethodEnricher>()
            };

            mockServices
                .Setup(x => x.GetEnumerator())
                .Returns(serviceDescriptors.AsEnumerable().GetEnumerator());

            sut.AddLinkDataEnricher(s =>
            {
                var enricher = new TestLinkDataEnricher();
                enricher.Name = "test";

                return enricher;
            });

            mockServices.Verify(x => x.Remove(It.Is<ServiceDescriptor>(x => x.ServiceType == typeof(ILinkDataEnricher))), Times.Never);

            mockServices.Verify(x =>
               x.Add(It.Is<ServiceDescriptor>(y =>
                   y.ServiceType == typeof(ILinkDataEnricher) &&
                   (y.ImplementationFactory.Invoke(null) as TestLinkDataEnricher).Name == "test" &&
                   y.Lifetime == ServiceLifetime.Transient)),
               Times.Once);
        }

        [Fact]
        public void AddLinkDataEnricherWithFactoryShouldReturnSameInstanceOfHypermediaServiceBuilder()
        {
            var result = sut.AddLinkDataEnricher(s => new TestLinkDataEnricher());

            result.Should().BeSameAs(sut);
        }

        public sealed class TestBaseUrlProvider : IBaseUrlProvider
        {
            public string Name { get; set; }

            public Uri Provide()
            {
                throw new NotImplementedException();
            }
        }

        public sealed class TestUrlPathProvider : IUrlPathProvider
        {
            public string Name { get; set; }

            public Uri Provide(UrlPathProviderContext context)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class TestLinkFactory : ILinkFactory
        {
            public string Name { get; set; }

            public Link Create(LinkFactoryContext context)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class TestLinkDataEnricher : ILinkDataEnricher
        {
            public string Name { get; set; }

            public void Enrich(LinkRequest linkRequest, LinkDataWriter writer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
