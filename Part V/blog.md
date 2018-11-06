# ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量

![](C:\Users\Administrator\OneDrive\博客\GraphQL\Part III\images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL

---

## 字段

```c#
public class Item  
{
    public string Barcode { get; set; }

    public string Title { get; set; }

    public decimal SellingPrice { get; set; }
}
```

```c#
public class ItemType : ObjectGraphType<Item>  
{
    public ItemType()
    {
        Field(i => i.Barcode);
        Field(i => i.Title);
        Field(i => i.SellingPrice);
    }
}
```

```c#
public HelloWorldQuery()  
{
    ...
    ...

    Field<ItemType>(
        "item",
        resolve: context =>
        {
           return new Item {
                Barcode = "123",
                Title = "Headphone",
                SellingPrice = 12.99M
            };
        }
    ); 
}
```



## 参数

```c#
public class DataSource  
{
    public IList<Item> Items
    {
        get;
        set;
    }

    public DataSource()
    {
        Items = new List<Item>(){
            new Item { Barcode= "123", Title="Headphone", SellingPrice=50},
            new Item { Barcode= "456", Title="Keyboard", SellingPrice= 40},
            new Item { Barcode= "789", Title="Monitor", SellingPrice= 100}
        };
    }

    public Item GetItemByBarcode(string barcode)
    {
        return Items.First(i => i.Barcode.Equals(barcode));
    }
}
```

```c#
Field<ItemType>(
    "item",
    arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "barcode" }),
    resolve: context =>
    {
        var barcode = context.GetArgument<string>("barcode");
        return new DataSource().GetItemByBarcode(barcode);
    }
);
```

```json
query {  
  item (barcode: "123") {
    title
    selling price
  }
}
```

```
query {  
  item {
    title
    sellingPrice
  }
}
```

```c#
QueryArgument<NonNullGraphType<StringGraphType>> { Name = "barcode" }  
```



## 变量

```c#
public class GraphQLRequest
{
    public string Query { get; set; }
    public JObject Variables { get; set; }
}
```

```c#
var result = await _executor.ExecuteAsync(doc =>
{
    doc.Schema = _schema;
    doc.Query = request.Query;

    doc.Inputs = request.Variables.ToInputs();

}).ConfigureAwait(false);
```

```
query($barcode: String!){  
  item(barcode: $barcode){
    title
    sellingPrice
  }
}
```

