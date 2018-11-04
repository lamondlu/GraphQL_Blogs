# ASP.NET Core中使用GraphQL - 第一章 Hello World

## 前言

你是否已经厌倦了REST风格的API? 让我们来聊一下GraphQL。 GraphQL提供了一种声明式的方式从服务器拉取数据。你可以从GraphQL官网中了解到GraphQL的所有优点。在这一系列博客中，我将展示如何在ASP.NET Core中集成GraphQL, 并使用GraphQL作为你的API查询语言。

> 使用GraphQL的声明式查询，你可以自定义API返回的属性列表。这与REST API中每个API只返回固定字段不同。

## 安装GraphQL

为了在C#中使用GraphQL, GraphQL社区中提供了一个开源组件<code>graphql-dotnet</code>。本系列博客中我们都将使用这个组件。

首先我们创建一个空的ASP.NET Core App

```
dotnet new web --name chatper1
```

然后我们添加对<code>graphql-dotnet</code>库的引用

```
dotnet add package GraphQL
```

## 创建第一个Query

下面我们来创建一个<code>query</code>类, 我们将它命名为<code>HelloWorldQuery</code>。<code>graphql-dotnet</code>中，查询类都需要继承<code>ObjectGraphType</code>类，所以<code>HelloWorldQuery</code>的代码如下

```c#
using GraphQL.Types;
public class HelloWorldQuery : ObjectGraphType
{
    public HelloWorldQuery()
    {
        Field<StringGraphType>(
            name: "hello",
            resolve: context => "world"
        );
    }
}
```

这里你可能注意到我们使用了一个泛型方法<code>Field</code>，并传递了一个GraphQL的字符串类型<code>StringGraphType</code>来定义了一个<code>hello</code>字段, <code>resolve</code> 参数是一个Func委托，在其中定义了如何返回当前字段的值，这里我们是直接返回了一个字符串hello。

> 查询类中的返回字段都是定义在查询类的构造函数中的

现在我们一个有了一个查询类，下一步我们需要使用这个查询类构建一个结构(schema)。

在<code>Startup.cs</code>文件的<code>Configure</code>方法中，使用以下代码替换原有代码

```c#
var schema = new Schema {
    Query = new HelloWorldQuery() 
};

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

- <code>DocumentExecuter</code> 类的<code>ExecuteAsync</code>方法中我们定义Action委托，并通过这个委托设置了一个<code>ExecutionOptions</code>对象。这个对象初始化了我们定义的结构(schema), 并执行了我们定义的查询字符串。
- <code>doc.Query</code>定义了一个查询字符串
- 最终查询执行的结果会通过<code>DocumentWriter</code>类实例的<code>Write</code>被转换成一个JSON字符串

下面我们来运行一下这个程序

```
dotnet run
```

你将在浏览器中看到以下结果

```json
{
  "data": {
    "hello": "world"
  }
}
```

从以上的例子中，你会发现使用GraphQL并不像想象中那么难。下面我们可以在<code>HelloWorldQuery</code>类的构造函数中再添加一个字段<code>howdy</code>, 并指定这个字段会返回一个字符串<code>universe</code>。

```
Field<StringGraphType>(
    name: "howdy",
    resolve: context => "universe"
); 
```

然后我们继续修改<code>Startup</code>类中的<code>Configure</code>方法, 修改我们之前定义的query

```c#
var schema = new Schema { 
    Query = new HelloWorldQuery()
};

app.Run(async (context) =>
{
    var result = await new DocumentExecuter()
        .ExecuteAsync(doc =>
    	{
        	doc.Schema = schema;
        	doc.Query = @"
            	query {
                	hello
                    howdy
                }
        	";
    	}).ConfigureAwait(false);

    var json = new DocumentWriter(indent: true)
        .Write(result)
    await context.Response.WriteAsync(json);
});
```

重新启动项目后，结果如下

```json
{
  "data": {
    "hello": "world",
    "howdy": "universe"
  }
}
```

## 总结

本篇我们只是接触了GraphQL的一些皮毛，你可能会对GraphQL声明式行为有很多问题，没有关系，后续博客中，我们慢慢解开GraphQL的面纱。下一篇我们将介绍如何创建一个中间件(Middleware)