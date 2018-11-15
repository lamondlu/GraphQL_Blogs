

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

然后我们需要在<code>IDataStore</code>中定义6个新的方法，并在<code>DataStore</code>中实现它们。

##### IDataStore

```c#
Task<IEnumerable<Order>> GetOrdersAsync();

Task<IEnumerable<Customer>> GetCustomersAsync();

Task<Customer> GetCustomerByIdAsync(int customerId);

Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);

Task<Order> AddOrderAsync(Order order);

Task<Customer> AddCustomerAsync(Customer customer);
```

##### DataStore

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

public async Task<Order> AddOrderAsync(Order order)  
{
    var addedOrder = await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();
    return addedOrder.Entity;
}

public async Task<Customer> AddCustomerAsync(Customer customer)  
{         
    var addedCustomer = await _context.Customers.AddAsync(customer);
    await _context.SaveChangesAsync();
    return addedCustomer.Entity;
}
```

添加完以上代码之后，我们就需要定义添加订单和顾客的输入类型了。还记得在上一章中我们如何添加货物的么？我们添加了一个<code>ItemInputType</code>类，定义了添加货物需要收集的字段，所以这里同理，我们也需要为订单和顾客定义对应的<code>InputObjectGraphType</code>。

##### OrderInputType

```c#
public class OrderInputType : InputObjectGraphType {  
    public OrderInputType()
    {
        Name = "OrderInput";
        Field<NonNullGraphType<StringGraphType>>("tag");
        Field<NonNullGraphType<DateGraphType>>("createdAt");
        Field<NonNullGraphType<IntGraphType>>("customerId");
    }
}
```

##### CustomerInputType

```c#
public class CustomerInputType : InputObjectGraphType {  
    public CustomerInputType()
    {
        Name = "CustomerInput";
        Field<NonNullGraphType<StringGraphType>>("name");
        Field<NonNullGraphType<StringGraphType>>("billingAddress");
    }
}
```

当前添加以上代码之后，我们还需要在<code>Startup</code>类中注册这几个新类型

```c#
public void ConfigureServices(IServiceCollection services)  
{ 
    ....
    ....
    services.AddScoped<CustomerType>();
    services.AddScoped<CustomerInputType>();
    services.AddScoped<OrderType>();
    services.AddScoped<OrderInputType>();
}
```

如果现在启动项目，你会得到以下错误

```
Failed to call Activator.CreateInstance. Type: chapter1.OrderType
```

这里的问题是在<code>InventorySchema</code>构造函数中的注入没起作用, 原因是<code>GraphQL</code>在解决依赖的时候，只能处理一层, 这里<code>OrderType</code>和<code>CustomerType</code>是2层的关系。如果想解决这个问题，我们需要在<code>Startup</code>中再注册一个依赖解决器。

```c#
services.AddScoped<IDependencyResolver>(s => 
    new FuncDependencyResolver(s.GetRequiredService));  
```

修改完成之后我们还需要修改<code>InventorySchema</code>, 在构造函数中将依赖解决器注入。

```c#
public class InventorySchema: Schema {  
    public InventorySchema(IDependencyResolver resolver): base(resolver) {
        Query = resolver.Resolve<InventoryQuery>();
        Mutation = resolver.Resolve<InventoryMutation>();
    }
}
```

现在再次启动项目，程序不报错了。

### 最终效果

下面我们首先创建一个<code>Customer</code>

![1542032211238](C:\Users\Administrator\OneDrive\博客\GraphQL\Part VIII\images\1542032211238.png)



然后我们继续创建2个<code>Order</code>

![1542032350492](C:\Users\Administrator\OneDrive\博客\GraphQL\Part VIII\images\1542032350492.png)

![1542032364396](C:\Users\Administrator\OneDrive\博客\GraphQL\Part VIII\images\1542032364396.png)

最后我们来查询一下刚才创建的数据是否存在

![1542032443893](C:\Users\Administrator\OneDrive\博客\GraphQL\Part VIII\images\1542032443893.png)

数据读取正确，这说明我们的数据添加成功了。

[本文源代码： https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20VIII](https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20VIII)