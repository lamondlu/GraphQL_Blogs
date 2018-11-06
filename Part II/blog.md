# ASP.NET Core中使用GraphQL - 第二章 中间件

前文：[ASP.NET Core中使用GraphQL - 第一章 Hello World](https://mp.weixin.qq.com/s?__biz=MzUyMzk0ODE2NQ==&mid=2247483872&idx=1&sn=2579e3e42d0ba666ecac71dfb11c1df0&chksm=fa35980acd42111c726baf927cfedb5748ef0dfc275bc31141b541c95d005d32c4f27253523d&token=2044683521&lang=zh_CN#rd)

------

![](C:\Users\Lamond Lu\OneDrive\博客\GraphQL\Part II\images\banner-8-1100x550.jpg)

### 中间件

如果你熟悉ASP.NET Core的中间件，你可能会注意到之前的博客中我们已经使用了一个中间件，

```c#
app.Run(async (context) =>
{
    var result = await new DocumentExecuter()
        .ExecuteAsync(doc =>
        {
            doc.Schema = schema;
            doc.Query = @"
                query {
                    hello
                }
            ";
        }).ConfigureAwait(false);

    var json = new DocumentWriter(indent: true)
        .Write(result)
    await context.Response.WriteAsync(json);
});
```

这个中间件负责输出了当前查询的结果。

> 中间件的定义：
>
> 中间件是装载在应用程序管道中的组件，负责处理请求和响应，每一个中间件
> - 可以选择是否传递请求到应用程序管道中的下一个组件
> - 可以在应用程序管道中下一个组件运行前和运行后进行一些操作
> 
> 来源： [Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?tabs=aspnetcore2x#what-is-middleware)

实际上中间件是一个委托，或者更精确的说是一个请求委托(Request Delegate)。 正如他的名字一样，中间件会处理请求，并决定是否将他委托到应用程序管道中的下一个中间件中。在我们前面的例子中，我们使用<code>IApplicationBuilder</code>类的<code>Run()</code>方法配置了一个请求委托。

### 使用动态查询体替换硬编码查询体

在我们之前的例子中，中间件中的代码非常简单，它仅是返回了一个固定查询的结果。然而在现实场景中，查询应该是动态的，因此我们必须从请求中读取查询体。

在服务器端，每一个请求委托都可以接受一个<code>HttpContext</code>参数。如果一个查询体是通过POST请求发送到服务器的，你可以很容易的使用如下代码获取到请求体中的内容。

```c#
string body;  
using (var streamReader = new StreamReader(httpContext.Request.Body))  
{
    body = await streamReader.ReadToEndAsync();
}
```

在获取请求体内容之前，为了不引起任何问题，我们需要先检测一些当前请求

- 是否是一个<code>POST</code>请求
- 是否使用了特定的Url, 例如<code> /api/graphql</code>

因此我们需要对代码进行调整。

```c#
if(context.Request.Path.StartsWithSegments("/api/graphql") 
   && string.Equals(context.Request.Method, 
                    "POST", 
                    StringComparison.OrdinalIgnoreCase))  
{
    string body;
    using (var streamReader = new StreamReader(context.Request.Body))
    {
        body = await streamReader.ReadToEndAsync();
    }

....
....
....
```

一个请求体可以包含很多字段，这里我们约定传入<code>graphql</code>查询体字段名称是<code>query</code>。因此我们可以将请求体中的JSON字符串转换成一个包含<code>Query</code>属性的复杂类型。

这个复杂类型代码如下：

```c#
public class GraphQLRequest
{
    public string Query { get; set; }
}
```

下一步我们要做的就是，反序列化当前请求体的内容为一个<code>GraphQLRequest</code>类型的实例。这里我们需要使用<code>Json.Net</code>中的静态方法<code>JsonConvert.DeserializeObjct</code>来替换之前的硬编码的查询体。

```c#
var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

var result = await new DocumentExecuter().ExecuteAsync(doc =>
{
    doc.Schema = schema;
    doc.Query = request.Query;
}).ConfigureAwait(false);
```

在完成以上修改之后，<code>Startup.cs</code>文件的<code>Run</code>方法应该是这个样子的。

```c#
app.Run(async (context) =>
{
    if (context.Request.Path.StartsWithSegments("/api/graphql")
        && string.Equals(context.Request.Method, 
                         "POST", 
                         StringComparison.OrdinalIgnoreCase))
    {
        string body;
        using (var streamReader = new StreamReader(context.Request.Body))
        {
            body = await streamReader.ReadToEndAsync();

            var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);
            var schema = new Schema { Query = new HelloWorldQuery() };

            var result = await new DocumentExecuter()
                .ExecuteAsync(doc =>
            {
                doc.Schema = schema;
                doc.Query = request.Query;
            }).ConfigureAwait(false);

            var json = new DocumentWriter(indent: true)
                .Write(result);
            await context.Response.WriteAsync(json);
        }
    }
});
```

### 最终效果

现在我们可以使用POSTMAN来创建一个POST请求, 请求结果如下：

![](.\images\1541413123554.png)

结果正确返回了。

[本篇源代码： https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20II](https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20II)

