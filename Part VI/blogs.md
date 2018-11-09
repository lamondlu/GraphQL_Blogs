# ASP.NET Core中使用GraphQL - 第六章  使用EF Core持久化数据

![](images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL
- ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量

本篇中我讲演示如何配置持久化仓储，这里原文中是使用的<code>Postgres</code>, 这里我改用了<code>EF Core</code>。

之前我们编写了一个<code>DataStore</code>类，里面硬编码了一个数据集合，这里我们希望改用依赖注入的方式进行解耦，所以首先我们需要创建一个抽象接口<code>IDataStore</code>。

```c#
public interface IDataStore
{
    IEnumerable<Item> GetItems();
    Item GetItemByBarcode(string barcode);
}
```

由于接下来我们需要使用<code>EF Core</code>, 所以这里我们需要添加一个<code>EF Core</code>的上下文类<code>ApplicationDbContext</code>。

```c#
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<Item> Items { get; set; }
}
```

下面我们修改<code>DataStore</code>类, <code>DataStore</code>应该实现<code>IDataStore</code>接口, 其代码如下

```c#
public class DataStore : IDataStore
{
    private ApplicationDbContext _applicationDbContext;

    public DataStore(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Item GetItemByBarcode(string barcode)
    {
        return _applicationDbContext.Items.First(i => i.Barcode.Equals(barcode));
    }

    public IEnumerable<Item> GetItems()
    {
        return _applicationDbContext.Items;
    }
}
```

接下来，我们要在<code>Startup.cs</code>类中

````c#

````

