

# ASP.NET Core中使用GraphQL - 第七章  Mutation

![](images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL
- ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量
- ASP.NET Core中使用GraphQL - 第六章 使用EF Core作为持久化仓储

---

在前面几篇中，我们已经介绍了如何使用GraphQL获取数据。那么如何使用GraphQL进行数据的添加，删除，修改操作的。这里我们需要引入GraphQL中的<code>Mutation</code>。

我们继续编写新代码之前，我们需要先整理一下当前的项目代码。这里我们将<code>HelloWorldQuery</code>类改名为<code>InventoryQuery</code>类, 并将<code>HelloWorldSchema</code>类改名为<code>InventorySchema</code>。然后我们将<code>hello</code>和<code>howdy</code>两个字段移除掉。

在GraphQL中, 一个<code>Mutation</code>类型也是继承自<code>ObjectGraphType</code>类。在以下代码中，<code>createItem</code>字段在服务器端创建了一个货物并返回了它的内容。

##### InventoryMutation

```c#
public class InventoryMutation : ObjectGraphType  
{
    public InventoryMutation(IDataStore dataStore)
    {         
        Field<ItemType>(
            "createItem",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<ItemInputType>> { Name = "item" }
            ),
            resolve: context =>
            {
                var item = context.GetArgument<Item>("item");
                return dataStore.AddItem(item);
            });
    }
}
```

以上代码中我们引入了一个新的<code>ItemInputType</code>类作为查询参数。在第五章中，我们已经创建过一个标量类型的参数。但是针对复杂类型，我们使用不同的方式。因此，这里我们创建了一个新的类<code>ItemInputType</code>。其代码如下：

##### ItemInputType

```c#
public class ItemInputType : InputObjectGraphType  
{
    public ItemInputType()
    {
        Name = "ItemInput";
        Field<NonNullGraphType<StringGraphType>>("barcode");
        Field<NonNullGraphType<StringGraphType>>("title");
        Field<NonNullGraphType<DecimalGraphType>>("sellingPrice");
    }
}
```

为了将新的货物记录添加到数据库，我们还需要修改<code>IDataStore</code>接口，添加一个<code>AddItem</code>的方法，并在<code>DataStore</code>类中实现它。

##### DataStore

```c#
public async Task<Item> AddItem(Item item)  
{
    var addedItem = await _context.Items.AddAsync(item);
    await _context.SaveChangesAsync();
    return addedItem.Entity;
}
```

这里请注意<code>AddItem</code>的方法签名，在添加完成之后，我们将添加成功的货物记录返回了。因此我们可以查询新添加对象的内嵌字段

>Just like in queries, if the mutation field returns an object type, you can ask for nested fields. This can be useful for fetching the new state of an object after an update. - [GraphQl Org.](http://graphql.org/learn/queries/#mutations)
>
>和查询一样，如果<code>mutation</code>字段返回一个对象类型，你就可以查询它的内嵌字段。这对于获取一个更新后对象的新状态非常有用。

在我们运行程序之前，我们还如要在控制反转容器中注册<code>ItemInputType</code>和<code>InventoryMutation</code>。

##### Startup

```c#
services.AddScoped<ItemInputType>();  
services.AddScoped<InventoryMutation>();  
```

最后我们需要在<code>InventorySchema</code>的构造函数中，注入<code>InventoryMutation</code>


##### InventorySchame 

```c#
public class InventorySchema : Schema  
{
    public InventorySchema(InventoryQuery query, InventoryMutation mutation)
    {
        Query = query;
        Mutation = mutation;
    }
}
```

现在你可以运行程序了，这里我们运行如下的<code>mutation</code>

```
mutation {  
  createItem(item: {title: "GPU", barcode: "112", sellingPrice: 100}) {
    title
    barcode
  }
}
```

这段代码的意思是，我们将调用<code>createItem</code>的<code>mutation</code>, 将item保存到数据库，并会返回新增item的<code>title</code>和<code>barcode</code>属性。当然你也可以把添加的item对象放到<code>Query Variables</code>窗口中。

![1541860029324](C:\Users\Administrator\OneDrive\博客\GraphQL\Part VII\images\1541860029324.png)

