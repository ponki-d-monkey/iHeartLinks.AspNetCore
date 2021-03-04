# iHeartLinks.AspNetCore

[![Build status](https://dev.azure.com/marlon-hizole/iHeartLinks/_apis/build/status/iHeartLinks.AspNetCore.CI)](https://dev.azure.com/marlon-hizole/iHeartLinks/_build/latest?definitionId=18)

**iHeartLinks.AspNetCore** is a class library for implementing [HATEOAS](https://en.wikipedia.org/wiki/HATEOAS) in a RESTful API. It is an implementation of [iHeartLinks.Core](https://github.com/ponki-d-monkey/iHeartLinks.Core) for ASP.NET core applications. It is simple, extensible, and contains methods to easily add hypermedia links to an API response.

## Getting started

Install the package from [NuGet.org](https://www.nuget.org/packages/iHeartLinks.AspNetCore/).

```powershell
PM> Install-Package iHeartLinks.AspNetCore
```

Once installed, add the dependencies in the `Startup` code.

```csharp
using iHeartLinks.AspNetCore;

public class Startup
{
  public void ConfigureServices(IServicesCollection services)
  {
    services.AddHateoas();
  }
}
```

Implement `IHypermediaDocument` in a model.

```csharp
using iHeartLinks.Core;

public class Person : IHypermediaDocument
{
  [JsonPropertyName("_links")]
  public IDictionary<string, Link> Links { get; private set; }

  public string Name { get; set; }

  public void AddLink(string rel, Link link)
  {
    (Links ??= new Dictionary<string, Link>()).Add(rel, link);
  }
}
```

Inject `IHypermediaService` into any controller to add links to a model that implements `IHypermediaDocument`.

```csharp
using iHeartLinks.AspNetCore;
using iHeartLinks.Core;

[Route("api/[controller]")]
[ApiController]
public class PersonController
{
  private readonly IHypermediaService hypermediaService;

  public PersonController(IHypermediaService hypermediaService)
  {
    this.hypermediaService = hypermediaService;
  }

  [HttpGet]
  [Route("{id}", Name = "GetPerson")]
  public IActionResult Get(long id)
  {
    var model = new Person
    {
      Name = "Juan Dela Cruz"
    };

    return Ok(hypermediaService
      .AddSelf(model)
      .AddRouteLink("update", "UpdatePerson", new { id })
      .Document);
  }

  [HttpPost]
  [Route("{id}", Name = "UpdatePerson")]
  public IActionResult Update(long id, Person person)
  {
    throw new NotImplementedException();
  }
}
```

The code above will produce an API response in JSON format like the example below.

```json
{
  "name": "Juan Dela Cruz",
  "_links": {
    "self": {
      "href": "https://your.api.com/person/1"
    },
    "update": {
      "href": "https://your.api.com/person/1"
    }
  }
}
```

## Adding links

### Conditional

Links can be added to a model by calling the `AddRouteLink()` method like in the example above. This method also has an overload that allows adding links based on a condition.

```csharp
hypermediaService
  .AddSelf(model)
  .AddRouteLink("update", "UpdatePerson", new { id }, m => m.IsActive)
  .Document;
```

The link with _"update"_ `rel` will only be added to the model if `IsActive` is `true`.

Multiple links can also be added based on a single condition.

```csharp
hypermediaService
  .AddSelf(model)
  .AddLinksPerCondition(m => m.IsActive, b => b
    .AddRouteLink("update", "UpdatePerson", new { id })
    .AddRouteLink("deactivate", "DeactivatePerson", new { id }))
  .AddRouteLink("activate", "ActivatePerson", new { id }, m => !m.IsActive)
  .Document;
```

This feature is part of [iHeartLinks.Core](https://github.com/ponki-d-monkey/iHeartLinks.Core).

### External

To add an external link, use the `AddLink()` method.

```csharp
hypermediaService
  .AddSelf(model)
  .AddLink("profile", "https://social-media.example.com/api/profile/5B94C11F-FB12-415D-9F75-8E1832F0D6F8"))
  .Document;
```

This feature is part of [iHeartLinks.Core](https://github.com/ponki-d-monkey/iHeartLinks.Core).

## Models

As mentioned above, models need to implement `IHypermediaDocument`. A base class that implements `IHypermediaDocument` can be ceated and inherited by the models. The `Person` class above can be refactored this way.

```csharp
using iHeartLinks.Core;

public class DefaultHypermediaDocument : IHypermediaDocument
{
  [JsonPropertyName("_links")]
  public IDictionary<string, Link> Links { get; private set; }

  public void AddLink(string rel, Link link)
  {
    (Links ??= new Dictionary<string, Link>()).Add(rel, link);
  }
}

public class Person : DefaultHypermediaDocument
{
  public string Name { get; set; }
}
```

Implementing `IHypermediaDocument` in models lets the package consumer define and configure how these classes will be serialized - whether as `JSON` or `XML` or to use _camel casing_ or not. This is helpful if the application is using an external package to serialize the API response like _Newtonsoft.Json_.

## Configuration

### Extended link

By default, only _rel_ and _href_ will be returned by `IHypermediaService`. If an _HTTP method_ is needed, the `IHypermediaService` can be configured to return a link object that contains a `Method` property.

```csharp
using iHeartLinks.AspNetCore;
using iHeartLinks.AspNetCore.Extensions;

public class Startup
{
  public void ConfigureServices(IServicesCollection services)
  {
    services.AddHateoas(b => b.UseExtendedLink());
  }
}
```

Calling the `UseExtendedLink()` method also enables retrieval of _templated links_.

### Custom href

By default, the _href_ of a link is an absolute URL where the base URL is the scheme and server values of the current request. To give a custom base URL, configure `IHypermediaService` in the `Startup` code.

```csharp
using iHeartLinks.AspNetCore;

public class Startup
{
  public void ConfigureServices(IServicesCollection services)
  {
    services.AddHateoas(b => b.UseCustomBaseUrlHref("https://your.custom.url"));
  }
}
```

For backward compatibility, a relative URL _href_ is also supported.

```csharp
using iHeartLinks.AspNetCore;

public class Startup
{
  public void ConfigureServices(IServicesCollection services)
  {
    services.AddHateoas(b => b.UseRelativeUrlHref());
  }
}
```

## Extensions

Features that can be turned on/off have been moved to the `iHeartLinks.AspNetCore.Extensions` namespace. See the _Configuration_ section above to understand how to enable these features.

### Templated

Templated links can be added to a model by calling the method `AddRouteTemplate()` method.

```csharp
using iHeartLinks.AspNetCore;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.Core;

[Route("api/[controller]")]
[ApiController]
public class PersonController
{
  private readonly IHypermediaService hypermediaService;

  public PersonController(IHypermediaService hypermediaService)
  {
    this.hypermediaService = hypermediaService;
  }

  [HttpGet]
  [Route("", Name = "GetPeople")]
  public IActionResult Get()
  {
    var collectionModel = new CollectionModel<Person>();

    return Ok(hypermediaService
      .AddSelf(collectionModel)
      .AddRouteTemplate("get", "GetPerson")
      .Document);
  }

  [HttpGet]
  [Route("{id}", Name = "GetPerson")]
  public IActionResult Get(long id)
  {
    throw new NotImplementedException();
  }
}
```

The code above will produce an API response in JSON format like the example below.

```json
{
  "items": [],
  "_links": {
    "self": {
      "href": "https://your.api.com/person",
      "method": "GET"
    },
    "get": {
      "href": "https://your.api.com/person/{id}",
      "method": "GET",
      "templated": true
    }
  }
}
```

Take note, calling the `AddRouteTemplate()` method without enabling templated links will result in an exception.

### External

To add an external link with an _HTTP method_, use the `AddLink()` method in the `iHeartLinks.AspNetCore.Extensions` namespace.

```csharp
hypermediaService
  .AddSelf(model)
  .AddLink("profile", "https://social-media.example.com/api/profile/5B94C11F-FB12-415D-9F75-8E1832F0D6F8", "GET"))
  .Document;
```

## Extending

If there is a need to return custom link properties from `IHypermediaService`, this can be achieved by doing the following.

Create a custom link class that inherits from [Link](src/iHeartLinks.Core/Link.cs). This is demonstrated in the code below. In addition, see [HttpLink](src/iHeartLinks.AspNetCore/Extensions/HttpLink.cs) as a guide.

```csharp
using iHeartLinks.Core;

public class CustomLink : Link
{
  public CustomLink(string href, string title)
    : base(href)
  {
    Title = title;
  }

  public new string Href => base.Href;

  public string Title { get; }
}
```

Create a custom enricher by implementing the [ILinkDataEnricher](src/iHeartLinks.AspNetCore/Enrichers/ILinkDataEnricher.cs) interface. See [HttpMethodEnricher](src/iHeartLinks.AspNetCore/Extensions/HttpMethodEnricher.cs) as a guide.

The custom enricher must be registered in the service collections in the `Startup` code. It can added by using the `HypermediaServiceBuilder` object.

```csharp
using iHeartLinks.AspNetCore;

public class Startup
{
  public void ConfigureServices(IServicesCollection services)
  {
    services.AddHateoas(b => b.AddLinkEnricher<TitleEnricher>());
  }
}
```

Finally, the custom link properties added in the custom enricher must be mapped in a custom [ILinkFactory](src/iHeartLinks.AspNetCore/LinkFactories/ILinkFactory.cs). See [HttpLinkFactory](src/iHeartLinks.AspNetCore/Extensions/HttpLinkFactory.cs) as a guide. Similarly, the custom link factory must be registered in the service collections in the `Startup` code.

## Difference with other libraries

A noticeable difference of **iHeartLinks.AspNetCore** to other HATEOAS libraries is it add links within controller actions. It does not require developers to configure models or policies centrally (e.g. in a startup code). This design decision was made to create a relationship between controller actions and links instead of models and links. One benefit of this is to easily track where links are being added. This way, if there is a missing link or an incorrect link was added to a model, it is quicker to identify in which controller action the issue occurred.

Additionally, conflicts with links added to a model are less likely to happen. The same model can be used in more than one controller action and because adding links does not depend on the type of models, different instances of the same model used in different controller actions can be guaranteed to have different links based on how links were added in the controller actions.

Finally, **iHeartLinks.AspNetCore** is all about adding links to models. It does not take care of serialization, reading/writing of HTTP headers, and so on.

## Core library

**iHeartLinks.AspNetCore** depends on **iHeartLinks.Core**. To understand more about the core library, go to the [iHeartLinks.Core](https://github.com/ponki-d-monkey/iHeartLinks.Core) repository.
