
using GraphQL.Types;
using System.Collections.Generic;

namespace chapter1
{

    public class InventoryQuery : ObjectGraphType
    {
        public InventoryQuery(IDataStore dataStore)
        {
            Field<ItemType>(
                "item",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "barcode" }),
                resolve: context =>
                {
                    var barcode = context.GetArgument<string>("barcode");
                    return dataStore.GetItemByBarcode(barcode);
                }
            );

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
        }
    }
}