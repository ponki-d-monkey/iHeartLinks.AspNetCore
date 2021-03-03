using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Enrichers
{
    public sealed class HttpMethodEnricherTests
    {
        private const string TestId = "TestId";
        private const string HttpMethodKey = "httpMethod";
        private const string TestHttpMethod = "GET";

        private readonly IDictionary<string, string> keyParts;
        private readonly LinkKey linkKey;

        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter writer;

        private readonly List<object> endpointMetadata;
        private readonly AttributeRouteInfo attributeRouteInfo;
        private readonly Mock<IActionDescriptorCollectionProvider> mockProvider;

        private readonly HttpMethodEnricher sut;

        public HttpMethodEnricherTests()
        {
            keyParts = new Dictionary<string, string>
            {
                { LinkKey.IdKey, TestId }
            };

            linkKey = new LinkKey(keyParts);

            context = new LinkFactoryContext();
            writer = new LinkDataWriter(context);

            endpointMetadata = new List<object> { new HttpMethodMetadata(new string[] { TestHttpMethod }) };
            attributeRouteInfo = new AttributeRouteInfo { Name = TestId };

            mockProvider = new Mock<IActionDescriptorCollectionProvider>();
            SetupProvider();

            sut = new HttpMethodEnricher(mockProvider.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenProviderIsNull()
        {
            Action action = () => new HttpMethodEnricher(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("provider");
        }

        [Fact]
        public void EnrichShouldThrowArugmentNullExceptionWhenLinkKeyIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkKey");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(linkKey, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteHttpMethodValue()
        {
            sut.Enrich(linkKey, writer);

            var result = context.Get(HttpMethodKey);
            result.Should().Be(TestHttpMethod);
        }

        [Fact]
        public void EnrichShouldNotWriteHttpMethodWhenLinkKeyIdDoesNotExist()
        {
            SetupProvider(new ActionDescriptor
            {
                EndpointMetadata = new List<object> { new HttpMethodMetadata(new string[0]) },
                AttributeRouteInfo = attributeRouteInfo
            });

            sut.Enrich(linkKey, writer);

            var result = context.Get(HttpMethodKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteHttpMethodWhenNoHttpMethodFound()
        {
            SetupProvider(new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata,
                AttributeRouteInfo = new AttributeRouteInfo
                {
                    Name = "NonExistingId"
                }
            });

            sut.Enrich(linkKey, writer);

            var result = context.Get(HttpMethodKey);
            result.Should().BeNull();
        }

        private void SetupProvider()
        {
            SetupProvider(new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata,
                AttributeRouteInfo = attributeRouteInfo
            });
        }

        private void SetupProvider(ActionDescriptor actionDescriptor)
        {
            var endpointMetadata = new List<object>
            {
                new HttpMethodMetadata(new string[] { TestHttpMethod })
            };

            var actionDescriptors = new List<ActionDescriptor>
            {
                actionDescriptor
            };

            mockProvider
                .Setup(x => x.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(actionDescriptors.AsReadOnly(), 1));
        }
    }
}
