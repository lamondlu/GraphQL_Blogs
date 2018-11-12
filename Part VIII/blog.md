

# ASP.NET Core中使用GraphQL - 第八章  在GraphQL中处理一对多关系

![](images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL
- ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量
- ASP.NET Core中使用GraphQL - 第六章 使用EF Core作为持久化仓储
- ASP.NET Core中使用GraphQL - 第七章 Mutation

---

到目前为止我们一直在使用GraphQL操作单个实体。在本篇博文中，我们将使用GraphQL操作实体集合。

这里我们使用的场景是处理一个顾客的所有订单，顾客和订单之间的关系是一对多。一个顾客可以有多个订单，相应的一个订单只属于一个顾客。

### 数据库修改

下面我们首先创建2个新的类<code>Customer</code>和<code>Order</code>。

##### Customer

```c#
public class Customer
{
	public int CustomerId { get; set; }
	public string Name { get; set; }
	public string BillingAddress { get; set; }
	public IEnumerable<Order> Orders { get; set; }
}
```

##### Order

```c#
public class Order
{
	public int OrderId { get; set; }
	public string Tag { get; set; }
	public DateTime CreatedAt { get; set; }

	public Customer Customer { get; set; }
	public int CustomerId { get; set; }
}
```

然后我们修改<code>ApplicationDbContext</code>类，在<code>OnModelCreating</code>配置一下表的主外键。

```c#
modelBuilder.Entity<Customer>()
	.HasKey(p => p.CustomerId);
modelBuilder.Entity<Customer>().HasMany(p => p.Orders)
    .WithOne()
    .HasForeignKey(p => p.CustomerId);

modelBuilder.Entity<Order>().HasKey(p => p.OrderId);
```

最后我们使用如下命令创建迁移并更新数据库

```
dotnet ef migrations add OneToManyRelationship  
dotnet ef database update 
```

至此数据库修改完成。

### 添加GraphQL代码

下面我们需要添加GraphQL针对<code>Customer</code>和<code>Order</code>表的字段配置。

##### OrderType

```c#
public class OrderType: ObjectGraphType <Order> {  
    public OrderType(IDataStore dataStore) {
        Field(o => o.Tag);
        Field(o => o.CreatedAt);
        Field <CustomerType, Customer> ()
            .Name("Customer")
            .ResolveAsync(ctx => {
                return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);
            });
    }
}
```

##### CustomerType.cs

```c#
public class CustomerType: ObjectGraphType <Customer> {  
    public CustomerType(IDataStore dataStore) {
        Field(c => c.Name);
        Field(c => c.BillingAddress);
        Field <ListGraphType<OrderType> , IEnumerable<Order>> ()
            .Name("Orders")
            .ResolveAsync(ctx => {
                return dataStore.GetOrdersByCustomerIdAsync(ctx.Source.CustomerId);
            });
    }
}
```

为了查询所有的顾客和订单，我们还需要暴露出2个新的节点。所以我们修改在<code>InventoryQuery</code>构造函数中添加如下代码：

##### InventoryQuery

```c#
Field<ListGraphType<OrderType>, IEnumerable<Order>>()  
    .Name("Orders")
    .ResolveAsync(ctx =>
    {
        return dataStore.GetOrdersAsync();
    });

Field<ListGraphType<CustomerType>, IEnumerable<Customer>>()  
    .Name("Customers")
    .ResolveAsync(ctx =>
    {
        return dataStore.GetCustomersAsync();
    });
```

然后我们需要实现<code>IDataStore</code>中新添加的方法

```c#
public async Task<IEnumerable<Order>> GetOrdersAsync()
{
	return await _context.Orders
		.AsNoTracking()
		.ToListAsync();
}

public async Task<IEnumerable<Customer>> GetCustomersAsync()
{
	return await _context.Customers
		.AsNoTracking()
		.ToListAsync();
}

public async Task<Customer> GetCustomerByIdAsync(int customerId)
{
	return await _context.Customers
		.FindAsync(customerId);
}

public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
{
	return await _context.Orders
		.Where(o => o.CustomerId == customerId)
		.ToListAsync();
}
```







[本文源代码： https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20VIII](https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20VIII)