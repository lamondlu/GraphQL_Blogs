# ASP.NET Core中使用GraphQL - 第三章 依赖注入

![](images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- [ASP.NET Core中使用GraphQL - 第一章 Hello World](https://mp.weixin.qq.com/s?__biz=MzUyMzk0ODE2NQ==&mid=2247483872&idx=1&sn=2579e3e42d0ba666ecac71dfb11c1df0&chksm=fa35980acd42111c726baf927cfedb5748ef0dfc275bc31141b541c95d005d32c4f27253523d&token=1273994697&lang=zh_CN#rd)
- [ASP.NET Core中使用GraphQL - 第二章 中间件](https://mp.weixin.qq.com/s?__biz=MzUyMzk0ODE2NQ==&mid=2247483879&idx=1&sn=31650605046e8ce291b1aa40c00429f9&chksm=fa35980dcd42111b979c3abe58480dde9182198a3e5c6707ea0fa3d653889af748eed002cbc2&token=1273994697&lang=zh_CN#rd)

---

> <code>SOLID</code>原则中的<code>D</code>表示依赖倒置原则。这个原则的内容是：
>
> - 上层模块不应该直接依赖底层模块，而应该依赖其抽象
> - 抽象不应该依赖于细节, 细节应该依赖抽象
>
> 来源：[WIKIPEDIA](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

在一个模块中创建一个其他模块的实例会导致这个模块与其他模块之间的紧耦合。 为了让不同的模块解耦，我们需要遵循依赖倒置原则。按照这种原则，一个模块不会依赖于其他模块的实现，会依赖于其他模块的抽象，例如接口。

一个抽象会存在许多个实现。无论何时我们碰到一个抽象，我们都需要传递一个该抽象的实现。所以我们需要一个类来负责配置他们之间的映射，这里我们称这个类为依赖注入容器(dependency injection container)。

ASP.NET Core中已经内置了一个依赖注入容器。它使用起来非常简单。它不仅能够配置抽象接口与实现类之间的映射，还可以配置实现类实例的生命周期。

在我们之前的Hello World项目中，我们没有关注过实例的生命周期。到目前为止，我们会将所有实现类对象设置为了<code>Singleton</code>。

这里我们首先需要解除对<code>DocumentWriter</code>和<code>DocumentExecuter</code>类依赖。方法就是使用抽象接口<code>IDocumentWriter</code>和<code>IDocumentExecuter</code>替换<code>DocumentWriter</code>和<code>DocumentExecuter</code>。

```c#
public void ConfigureServices(IServiceCollection services)  
{
    services.AddSingleton<IDocumentWriter, DocumentWriter>();
    services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
}
```

对于<code>HelloWorldQuery</code>实例，我们没有编写任何抽象接口，所以这里我们简单的使用了其原始实现。

```c#
services.AddSingleton<HelloWorldQuery>(); 
```

当前的结构(Schema)中包含了一个<code>query</code>, 在后续的博文中我们还会添加<code>mutation</code>和其他字段，所以这里我们最好创建一个独立的类来设置它。所以这里我们创建了一个<code>HelloWorldSchema</code>类，它继承自<code>Schema</code>, 并在构造中注入了一个<code>HelloWorldQuery</code>实例。



```c#
public class HelloWorldSchema : Schema
{
    public HelloWorldSchema(HelloWorldQuery query)
    {
        Query = query;
    }
}
```

最后我们在<code>Startup.cs</code>文件的<code>Configure</code>方法中注入<code>HelloWorldSchame</code>

```c#
services.AddSingleton<ISchema, HelloWorldSchema>();  
```

> TIPS：<code>ISchema</code>是<code>graphql-dotnet</code>库中一个接口，<code>Schema</code>类实现了<code>ISchema</code>接口

现在我们将之前创建的中间件移到一个单独的类中，我们将它命名为<code>GraphQLMiddleware</code>， 其代码如下。

```c#
public class GraphQLMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDocumentWriter _writer;
    private readonly IDocumentExecuter _executor;
    private readonly ISchema _schema;

    public GraphQLMiddleware(RequestDelegate next, 
                             IDocumentWriter writer, 
                             IDocumentExecuter executor, 
                             ISchema schema)
    {
        _next = next;
        _writer = writer;
        _executor = executor;
        _schema = schema;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/api/graphql") 
            && string.Equals(httpContext.Request.Method, 
                             "POST", 
                             StringComparison.OrdinalIgnoreCase))
        {
            string body;
            using (var streamReader = new StreamReader(httpContext.Request.Body))
            {
                body = await streamReader.ReadToEndAsync();

                var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

                var result = await _executor.ExecuteAsync(doc =>
                {
                    doc.Schema = _schema;
                    doc.Query = request.Query;
                }).ConfigureAwait(false);

                var json = _writer.WriteToStringAsync(result);
                await httpContext.Response.WriteAsync(json);
            }
        }
        else
        {
            await _next(httpContext);
        }
    }
}
```

这里你会注意到我们是如何使用抽象接口来解耦的，在<code>GraphQLMiddleware</code>的构造函数中，我们注入了当前中间件所需的所有服务<code>IDocumentWriter</code>, <code>IDocumentExecuter</code>, 以及<code>ISchema</code>

最后我们需要将这个中间件注册到应用程序管道中。<code>IApplicationBuilder</code>接口提供了一个扩展方法<code>UseMiddleware</code>, 我们可以使用它来注册中间件。所以最终<code>Configure</code>方法中的代码如下：

```c#
public void Configure(IApplicationBuilder app, 
    IHostingEnvironment env)
{
    app.UseMiddleware<GraphQLMiddleware>();
}
```

现在我们重新使用POSTMAN来测试。

