
using GraphQL.Types;

namespace chapter1
{
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
        }
    }
}
