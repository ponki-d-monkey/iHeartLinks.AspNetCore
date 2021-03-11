using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
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
        private const string TestRouteName = "TestRouteName";
        private const string TestHttpMethod = "GET";

        private readonly LinkRequest request;

        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter writer;

        private readonly List<object> endpointMetadata;
        private readonly AttributeRouteInfo attributeRouteInfo;
        private readonly Mock<IActionDescriptorCollectionProvider> mockProvider;

        private readonly HttpMethodEnricher sut;

        public HttpMethodEnricherTests()
        {
            request = LinkRequestBuilder.CreateWithRouteName(TestRouteName);
            context = new LinkFactoryContext();
            writer = new LinkDataWriter(context);

            endpointMetadata = new List<object> { new HttpMethodMetadata(new string[] { TestHttpMethod }) };
            attributeRouteInfo = new AttributeRouteInfo { Name = TestRouteName };

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
        public void EnrichShouldThrowArugmentNullExceptionWhenRequestIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(request, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteHttpMethodValue()
        {
            sut.Enrich(request, writer);

            var result = context.Get(HttpMethodEnricher.HttpMethodKey);
            result.Should().Be(TestHttpMethod);
        }

        [Fact]
        public void EnrichShouldNotWriteHttpMethodWhenRequestRouteNameDoesNotExist()
        {
            SetupProvider(new ActionDescriptor
            {
                EndpointMetadata = new List<object> { new HttpMethodMetadata(new string[0]) },
                AttributeRouteInfo = attributeRouteInfo
            });

            sut.Enrich(request, writer);

            var result = context.Get(HttpMethodEnricher.HttpMethodKey);
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
                    Name = "NonExistingRouteName"
                }
            });

            sut.Enrich(request, writer);

            var result = context.Get(HttpMethodEnricher.HttpMethodKey);
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
