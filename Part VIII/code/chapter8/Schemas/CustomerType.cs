using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chapter1
{
    public class CustomerType : ObjectGraphType<Customer>
    {
        public CustomerType(IDataStore dataStore)
        {
            Field(c => c.Name);
            Field(c => c.BillingAddress);
            Field<ListGraphType<OrderType>, IEnumerable<Order>>()
                .Name("Orders")
                .ResolveAsync(ctx => {
                    return dataStore.GetOrdersByCustomerIdAsync(ctx.Source.CustomerId);
                });
        }
    }
}
