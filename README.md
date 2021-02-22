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
      "href": "https://your.api.com/person/1",
      "method": "GET"
    },
    "update": {
      "href": "https://your.api.com/person/1",
      "method": "POST"
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

### Templated

Templated links can be added to a model by calling the method below.

```csharp
hypermediaService
  .AddSelf(collectionModel)
  .AddRouteTemplate("get", "GetPerson")
  .Document
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

## Difference with other libraries

A noticeable difference of **iHeartLinks.AspNetCore** to other HATEOAS libraries is it add links within controller actions. It does not require developers to configure models or policies centrally (e.g. in a startup code). This design decision was made to create a relationship between controller actions and links instead of models and links. One benefit of this is to easily track where links are being added. This way, if there is a missing link or an incorrect link was added to a model, it is quicker to identify in which controller action the issue occurred.

Additionally, conflicts with links added to a model are less likely to happen. The same model can be used in more than one controller action and because adding links does not depend on the type of models, different instances of the same model used in different controller actions can be guaranteed to have different links based on how links were added in the controller actions.

Finally, **iHeartLinks.AspNetCore** is all about adding links to models. It does not take care of serialization, reading/writing of HTTP headers, and so on.

## Core library

**iHeartLinks.AspNetCore** depends on **iHeartLinks.Core**. To understand more about the core library, go to the [iHeartLinks.Core](https://github.com/ponki-d-monkey/iHeartLinks.Core) repository.
