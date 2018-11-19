

# ASP.NET Core中使用GraphQL - 最终章  Data Loader

![](images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL
- ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量
- ASP.NET Core中使用GraphQL - 第六章 使用EF Core作为持久化仓储
- ASP.NET Core中使用GraphQL - 第七章 Mutation
- ASP.NET Core中使用GraphQL - 第八章 在GraphQL中处理一对多关系
- ASP.NET Core中使用GraphQL - 第九章 在GraphQL中处理多对多关系

---

在之前的几章中，我们的<code>GraphQL</code>查询是没有优化过的。下面我们以<code>CustomerType</code>中的<code>orders</code>查询为例

##### *CustomerType.cs*

```c#
Field<ListGraphType<OrderType>, IEnumerable<Order>>()  
    .Name("Orders")
    .ResolveAsync(ctx =>
    {
        return dataStore.GetOrdersAsync();
    }); 
```

在这个查询中，我们获取了某个顾客中所有的订单， 这里如果你只是获取一些标量字段，那很简单。

但是如果需要获取一些关联属性呢？例如查询系统中的所有订单，在订单信息中附带顾客信息。

##### *OrderType*

```c#
public OrderType(IDataStore dataStore, IDataLoaderContextAccessor accessor)  
{
    Field(o => o.Tag);
    Field(o => o.CreatedAt);
    Field<CustomerType, Customer>()
        .Name("Customer")
        .ResolveAsync(ctx =>
        {            
            return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);  
        });
}
```

这里当获取<code>customer</code>信息的时候，系统会另外初始化一个请求，以便从数据仓储中查询订单相关的顾客信息。

如果你了解<code>dotnet cli</code>, 你可以针对以下查询，在控制台输出所有的EF查询日志

```
{
  orders{
    tag
    createdAt
    customer{
      name
      billingAddress
    }
  }
}
```

查询结果：

```json
{
  "data": {
    "orders": [
      {
        "tag": "XPS 13",
        "createdAt": "2018-11-11",
        "customer": {
          "name": "Lamond Lu",
          "billingAddress": "Test Address"
        }
      },
      {
        "tag": "XPS 15",
        "createdAt": "2018-11-11",
        "customer": {
          "name": "Lamond Lu",
          "billingAddress": "Test Address"
        }
      }
    ]
  }
}
```

产生日志如下：

```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (16ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [o].[OrderId], [o].[CreatedAt], [o].[CustomerId], [o].[CustomerId1], [o].[Tag]
      FROM [Orders] AS [o]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (6ms) [Parameters=[@__get_Item_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      
      SELECT TOP(1) [e].[CustomerId], [e].[BillingAddress], [e].[Name]
      FROM [Customers] AS [e]
      WHERE [e].[CustomerId] = @__get_Item_0
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (5ms) [Parameters=[@__get_Item_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      
      SELECT TOP(1) [e].[CustomerId], [e].[BillingAddress], [e].[Name]
      FROM [Customers] AS [e]
      WHERE [e].[CustomerId] = @__get_Item_0
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[2]
      Request finished in 864.2749ms 200
```

从日志上我们很清楚的看到，这个查询使用了3个查询语句，第一个语句查询所有的订单信息，第二个和第三个请求分别查询了2个订单的顾客信息。这里可以想象如果这里有N的订单，就会产生N+1个查询语句，这是非常不效率的。正常情况下我们其实可以通过2条语句就完成上述的查询，后面查询单个顾客信息其实可以整合成一条语句。

为了实现这个效果，我们就需要介绍一下<code>GraphQL</code>中的<code>DataLoader</code>。

<code>DataLoader</code>是<code>GraphQL</code>中的一个重要功能，它为<code>GraphtQL</code>查询提供了批处理和缓存的功能。

为了使用<code>DataLoader</code>, 我们首先需要在<code>Startup.cs</code>中注册2个新服务`IDataLoaderContextAccessor` 和`DataLoaderDocumentListener`

##### *Startup.cs*

```c#
services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();  
services.AddSingleton<DataLoaderDocumentListener>();  
```

如果你的某个<code>GraphQL</code>类型需要<code>DataLoader</code>, 你就可以在其构造函数中注入一个<code>IDataLoaderContextAccessor</code>接口对象。

但是为了使用<code>DataLoader</code>, 我们还需要将它添加到我们的中间件中。

##### *GraphQLMiddleware.cs*

```c#
public async Task InvokeAsync(HttpContext httpContext, ISchema schema, IServiceProvider serviceProvider)  
{
    ....
    ....
        
    var result = await _executor.ExecuteAsync(doc =>
    {
        ....
        ....
        doc.Listeners.Add(serviceProvider                                                             .GetRequiredService<DataLoaderDocumentListener>());
    }).ConfigureAwait(false);

    ....
    ....            
}
```

下一步，我们需要为我们的仓储类，添加一个新方法，这个方法可以根据顾客的id列表，返回所有的顾客信息。

##### *DataStore.cs*

```c#
public async Task<Dictionary<int, Customer>> GetCustomersByIdAsync(
    IEnumerable<int> customerIds,
    CancellationToken token)  
{
    return await _context.Customers
        .Where(i => customerIds.Contains(i.CustomerId))
        .ToDictionaryAsync(x => x.CustomerId);
}
```

然后我们修改<code>OrderType</code>类

##### *OrderType*

```c#
Field<CustomerType, Customer>()  
    .Name("Customer")
    .ResolveAsync(ctx =>
    {            
        var customersLoader = accessor.Context.GetOrAddBatchLoader<int, Customer>("GetCustomersById", dataStore.GetCustomersByIdAsync);
        return customersLoader.LoadAsync(ctx.Source.CustomerId);  
    });
```

完成以上修改之后，我们重新运行项目,  使用相同的<code>query</code>, 结果如下，查询语句的数量变成了2个，效率大大提高

```
info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 2.1.4-rtm-31024 initialized 'ApplicationDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: None
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (19ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [o].[OrderId], [o].[CreatedAt], [o].[CustomerId], [o].[CustomerId1], [o].[Tag]
      FROM [Orders] AS [o]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (10ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [i].[CustomerId], [i].[BillingAddress], [i].[Name]
      FROM [Customers] AS [i]
      WHERE [i].[CustomerId] IN (1)
```

> <code>DataLoader</code>背后的原理
>
> <code>GetOrAddBatchLoader</code>方法会等到所有查询的顾客id列表准备好之后才会执行，它会一次性把所有查询id的顾客信息都收集起来。 这种技术就叫做批处理，使用了这种技术之后，无论有多少个关联的顾客信息，系统都只会发出一次请求来获取所有数据。

[本文源代码： https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20X](https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20X乬)