using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class HttpMethodEnricherTests
    {
        private const string TestId = "TestId";
        private const string HttpMethodKey = "httpMethod";
        private const string TestHttpMethod = "GET";

        private readonly IDictionary<string, string> keyParts;
        private readonly LinkRequest linkRequest;

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
                { LinkRequest.IdKey, TestId }
            };

            linkRequest = new LinkRequest(keyParts);

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
        public void EnrichShouldThrowArugmentNullExceptionWhenLinkRequestIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkRequest");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(linkRequest, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteHttpMethodValue()
        {
            sut.Enrich(linkRequest, writer);

            var result = context.Get(HttpMethodKey);
            result.Should().Be(TestHttpMethod);
        }

        [Fact]
        public void EnrichShouldNotWriteHttpMethodWhenLinkRequestIdDoesNotExist()
        {
            SetupProvider(new ActionDescriptor
            {
                EndpointMetadata = new List<object> { new HttpMethodMetadata(new string[0]) },
                AttributeRouteInfo = attributeRouteInfo
            });

            sut.Enrich(linkRequest, writer);

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

            sut.Enrich(linkRequest, writer);

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
