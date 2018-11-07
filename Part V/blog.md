# ASP.NET Core中使用GraphQL - 第五章 字段, 参数, 变量

![](C:\Users\Administrator\OneDrive\博客\GraphQL\Part III\images\banner-8-1100x550.jpg)

ASP.NET Core中使用GraphQL

- ASP.NET Core中使用GraphQL - 第一章 Hello World
- ASP.NET Core中使用GraphQL - 第二章 中间件
- ASP.NET Core中使用GraphQL - 第三章 依赖注入
- ASP.NET Core中使用GraphQL - 第四章 GrahpiQL

---

## 字段

我们已经很好的理解了GraphQL中的字段。在之前<code>HelloWorldQuery</code>的例子中，我们添加了2个字段<code>hello</code>和<code>howdy</code>. 它们都是标量字段。正如GraphQL官网文档中声明的那样

> *"At its simplest, GraphQL is about asking for specific fields on objects"*
>
> 简单来说，GraphQL就是询问对象中的一些特定字段
>
> 来源: graphql.org

下面我们来为我们的实例程序添加一些复杂的类型。比如，现在我们需要编写一个库存系统，我们首先添加一个货物类<code>Item</code>, 其代码如下：

```c#
public class Item  
{
    public string Barcode { get; set; }

    public string Title { get; set; }

    public decimal SellingPrice { get; set; }
}
```

但是我们不希望直接针对这个对象创建查询，因为它不是一个<code>GraphQL</code>对象，它没有继承自<code>ObjectGraphType</code>, 为了创建一个<code>GraphQL</code>查询，我们需要创建一个新类<code>ItemType</code>, 它继承自<code>ObjectGraphType</code>类。

另外<code>ObjectGraphType</code>类是一个泛型类，所以这里我们需要指定它的泛型参数是<code>Item</code>

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

这里有2点需要注意。首先我们不在针对字段进行类型声明了。<code>GraphQL</code>库将实体类属性字段类型映射成<code>GraphQL</code>的内置类型。例如这里<code>Barcode</code>的类型<code>string</code>会被映射成<code>GraphQL</code>的内置类型<code>StringGraphType</code>。其次这里我们使用了Lambda表达式设置了实体类属性和<code>GraphQL</code>字段之间的映射， 这有点类似于数据库模型和ViewModel之间的转换的映射。

下一步，我们需要在<code>HelloWorldQuery</code>中注册<code>ItemType</code>。

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

这里我们暂时设置了一个硬编码的返回值。所以当查询<code>item</code>对象的时候，这个硬编码的返回值会输出出来。

现在我们启动项目，进入<code>GraphiQL</code>界面

![1541598835805](C:\Users\Administrator\OneDrive\博客\GraphQL\Part V\images\1541598835805.png)

首先我们设置查询为

```
query {
    item{
        barcode
        sellingPrice
    }
}
```

运行查询之后，结果是

```json
{
  "data": {
    "item": {
      "barcode": "123",
      "sellingPrice": 12.99
    }
  }
}
```

然后我们修改查询为

```
query {
    item{
        barcode
        sellingPrice
        title
    }
}
```

 运行查询之后，结果是

```json
{
  "data": {
    "item": {
      "barcode": "123",
      "sellingPrice": 12.99,
      "title": "Headphone"
    }
  }
}
```

这说明我们的<code>GraphQL</code>查询已经生效，api根据我们需要的字段返回了正确的返回值。

## 参数

这里我们可以使用参数去除前面的硬编码。

为了说明如何使用参数，这里我们首先创建一个数据源类<code>DataSource</code>, 其代码如下

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

这里除了<code>Items</code>集合，我们还添加了一个方法<code>GetItemByBarcode</code>, 这个方法可以根据传递的<code>barcode</code>参数返回第一个匹配的<code>Item</code>。

然后现在我们来修改之前的<code>item</code>查询, 添加一个<code>arguments</code>参数, 其代码如下：

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

<code>arguments</code>是一个参数列表，里面可以包含必填参数和选填参数。针对每个参数，我们都需要指定它对应的类型，这里<code>Name</code>属性是设置了当前参数的名称。

在<code>resolve</code>参数中, 你可以使用<code>context.GetArgument<T>()</code>方法获取查询中传递的参数值。

现在我们重新启动项目，并在<code>GraphiQL</code>中添加如下查询

```json
query {  
  item (barcode: "123") {
    title
    sellingPrice
  }
}
```

输出的查询结果

```json
{
  "data": {
    "item": {
      "title": "Headphone",
      "sellingPrice": 50
    }
  }
}
```

这个结果与我们预想的一样。

但是这时候如果我们不传递<code>barcode</code>参数

```
query {  
  item {
    title
    sellingPrice
  }
}
```

程序就会报错

```json
{
  "data": {
    "item": null
  },
  "errors": [
    {
      "message": "Error trying to resolve item.",
      "locations": [
        {
          "line": 2,
          "column": 3
        }
      ],
      "path": [
        "item"
      ],
      "extensions": {
        "code": "INVALID_OPERATION"
      }
    }
  ]
}
```

原因是当前<code>barcode</code>是一个可空项，程序查询时, <code>First</code>方法会报错。所以这时候我们可以使用<code>NonNullGraphType<T></code>来设置<code>barcode</code>为一个必填项。

```c#
QueryArgument<NonNullGraphType<StringGraphType>> { Name = "barcode" }  
```

这样重新启动项目后，继续使用之前报错的查询，<code>GraphiQL</code>就会给出校验错误。

![1541600687765](C:\Users\Administrator\OneDrive\博客\GraphQL\Part V\images\1541600687765.png)

## 变量

现在是时候将参数变成动态了。 我们不希望每次在查询中写死查询条件，我们希望这个查询参数是动态的，这时候我们就需要使用到变量。

首先，这里我们需要确保我们的<code>GraphQL</code>中间件可以接受参数，所以我们需要在<code>GraphQLRequest</code>类中添加一个参数变量

```c#
public class GraphQLRequest
{
    public string Query { get; set; }
    public JObject Variables { get; set; }
}
```

然后我们需要修改<code>GraphQLMiddleware</code>中间件的<code>InvokeAsync</code>方法, 在其中添加一行代码设置<code>doc.Inputs</code>

```c#
var result = await _executor.ExecuteAsync(doc =>
{
    doc.Schema = _schema;
    doc.Query = request.Query;

    doc.Inputs = request.Variables.ToInputs();

}).ConfigureAwait(false);
```

现在我们的<code>item</code>查询已经支持动态参数了，我们可以运行程序，在<code>GraphiQL</code>中设置如下查询

```
query($barcode: String!){  
  item(barcode: $barcode){
    title
    sellingPrice
  }
}
```

查询中变量是以$开头的, 后面需要加上变量类型，因为之前我们这是了<code>barcode</code>参数为必填项，所以<code>$barcode</code>变量我们也要设置成必填。变量的必填设置是在变量类型后添加一个!号。

最后，在<code>GraphiQL</code>中，你可以使用QUERY VARIABLES面板中输入参数的值。如下图所示，最终结果正确的返回了。

![1541601475281](C:\Users\Administrator\OneDrive\博客\GraphQL\Part V\images\1541601475281.png)

