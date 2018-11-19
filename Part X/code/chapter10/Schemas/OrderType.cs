
using chapter1.Models;
using chapter1.Schemas;
using GraphQL.DataLoader;
using GraphQL.Types;
using System.Collections.Generic;

namespace chapter1
{
    public class OrderType : ObjectGraphType<Order>
    {
        public OrderType(IDataStore dataStore, IDataLoaderContextAccessor accessor)
        {
            Field(o => o.Tag);
            Field(o => o.CreatedAt);
            Field<CustomerType, Customer>()
                .Name("Customer")
                .ResolveAsync(ctx =>
                {
                    var customersLoader = accessor.Context.GetOrAddBatchLoader<int, Customer>("GetCustomersById",
                        dataStore.GetCustomersByIdAsync);
                    return customersLoader.LoadAsync(ctx.Source.CustomerId);
                });

            Field<ListGraphType<OrderItemType>, IEnumerable<OrderItem>>()
                .Name("Items")
                .ResolveAsync(ctx =>
                {
                    return dataStore.GetOrderItemByOrderIdAsync(ctx.Source.OrderId);
                });
        }
    }
}
