

# ASP.NET Core中使用GraphQL - 第九章  在GraphQL中处理多对多关系

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

---

上一章中，我们介绍了如何在GraphQL中处理一对多关系，这一章，我们来介绍一下GraphQL中如何处理多对多关系。

我们继续延伸上一章的需求，上一章中我们引入了客户和订单，但是我们没有涉及订单中的物品。在实际需求中，一个订单可以包含多个物品，一个物品也可以属于多个订单，所以订单和物品之间是一个多对多关系。

为了创建订单和物品之间的关系，这里我们首先创建一个订单物品实体。

##### OrderItem

```c#
[Table("OrderItems")]
public class OrderItem
{
	public int Id { get; set; }

    public string Barcode { get; set; }

    [ForeignKey("Barcode")]
	public virtual Item Item { get; set; }

	public int Quantity { get; set; }

	public int OrderId { get; set; }

    [ForeignKey("OrderId")]
	public virtual Order Order { get; set; }
}
```

创建完成之后，我们还需要修改<code>Order</code>和<code>Item</code>实体, 添加他们与<code>OrderItem</code>之间的关系

##### Order

```c#
public class Order
{
	public int OrderId { get; set; }
	public string Tag { get; set; }
	public DateTime CreatedAt { get; set; }

	public Customer Customer { get; set; }
	public int CustomerId { get; set; }

	public virtual ICollection<OrderItem> OrderItems { get; set; }
}
```

##### Item

```c#
[Table("Items")]
public class Item
{
    [Key]
    public string Barcode { get; set; }

    public string Title { get; set; }

    public decimal SellingPrice { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; }
}
```

修改完成之后，我们使用如下命令创建数据库迁移脚本，并更新数据库

```
dotnet ef migrations add AddOrderItemTable
dotnet ef database update
```

迁移成功之后，我们可以添加一个新的<code>GraphQL</code>节点，使用这个新节点，我们可以向订单中添加物品。为了实现这个功能，我们首先需要为<code>OrderItem</code>实体添加它在<code>GraphQL</code>中对应的类型<code>OrderItemType</code>

##### OrderItemType

```c#
public class OrderItemType : ObjectGraphType<OrderItem>  
{
    public OrderItemType(IDataStore dateStore)
    {   
        Field(i => i.ItemId);      

        Field<ItemType, Item>().Name("Item").ResolveAsync(ctx =>
        {
            return dateStore.GetItemByIdAsync(ctx.Source.ItemId);
        });         

        Field(i => i.Quantity);

        Field(i => i.OrderId);

        Field<OrderType, Order>().Name("Order").ResolveAsync(ctx =>
        {
            return dateStore.GetOrderByIdAsync(ctx.Source.OrderId);
        });

    }
}
```

第二步，我们还需要创建一个<code>OrderItemInputType</code>来定义添加<code>OrderItem</code>需要哪些字段。

##### OrderItemInputType

```
public class OrderItemInputType : InputObjectGraphType  
{
    public OrderItemInputType()
    {
        Name = "OrderItemInput";
        Field<NonNullGraphType<IntGraphType>>("quantity");
        Field<NonNullGraphType<IntGraphType>>("itemId");
        Field<NonNullGraphType<IntGraphType>>("orderId");
    }
}
```

第三步，我们需要在<code>InventoryMutation</code>类中针对<code>OrderItem</code>添加新的<code>mutation</code>。

##### InventoryMutation

```c#
Field<OrderItemType, OrderItem>()  
    .Name("addOrderItem")
    .Argument<NonNullGraphType<OrderItemInputType>>("orderitem", "orderitem input")
    .ResolveAsync(ctx =>
    {
        var orderItem = ctx.GetArgument<OrderItem>("orderitem");
        return dataStore.AddOrderItemAsync(orderItem);
    });
```

第四步，我们需要在<code>IDataStore</code>接口中定义几个新的方法，并在<code>DataStore</code>类中实现他们

##### IDataStore

```c#
Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);

Task<Order> GetOrderByIdAsync(int orderId);

Task<IEnumerable<OrderItem>> GetOrderItemByOrderIdAsync(int orderId);
```

##### DataStore

```c#
public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
{
	var addedOrderItem = await _context.OrderItems.AddAsync(orderItem);
	await _context.SaveChangesAsync();
	return addedOrderItem.Entity;
}

public async Task<Order> GetOrderByIdAsync(int orderId)
{
    return await _context.Orders.FindAsync(orderId);
}

public async Task<IEnumerable<OrderItem>> GetOrderItemByOrderIdAsync(int orderId)
{
    return await _context.OrderItems
        .Where(o => o.OrderId == orderId)
        .ToListAsync();
}
```

第五步，我们来修改<code>OrderType</code>类，我们希望查询订单的时候，可以返回订单中的所有物品

```c#
public class OrderType : ObjectGraphType<Order>
{
    public OrderType(IDataStore dataStore)
    {
        Field(o => o.Tag);
        Field(o => o.CreatedAt);
        Field<CustomerType, Customer>()
        .Name("Customer")
        .ResolveAsync(ctx =>
        {
            return dataStore.GetCustomerByIdAsync(ctx.Source.CustomerId);
        });

        Field<OrderItemType, OrderItem>()
        .Name("Items")
        .ResolveAsync(ctx =>
        {
            return dataStore.GetOrderItemByOrderIdAsync(ctx.Source.OrderId);
        });
        }
    }
}
```

最后我们还需要在<code>Startup</code>类中注册我们刚定义的2个新类型

```c#
services.AddScoped<OrderItemType>();  
services.AddScoped<OrderItemInputType>();  
```

以上就是所有的代码修改。现在我们启动项目

首先我们先为之前添加的订单1, 添加两个物品



![1542146751163](C:\Users\Administrator\OneDrive\博客\GraphQL\Part IX\images\1542146751163.png)

![1542146790397](C:\Users\Administrator\OneDrive\博客\GraphQL\Part IX\images\1542146790397.png)

然后我们来调用查询Order的<code>query</code>, 结果中订单中物品正确显示了。

![1542146907622](C:\Users\Administrator\OneDrive\博客\GraphQL\Part IX\images\1542146907622.png)

[本文源代码： https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20IX](https://github.com/lamondlu/GraphQL_Blogs/tree/master/Part%20IX)