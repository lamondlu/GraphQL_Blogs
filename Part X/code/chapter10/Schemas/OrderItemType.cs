using chapter1.Models;
using GraphQL.Types;

namespace chapter1.Schemas
{
    public class OrderItemType : ObjectGraphType<OrderItem>
    {
        public OrderItemType(IDataStore dateStore)
        {
            Field<ItemType, Item>().Name("Item").Resolve(ctx =>
            {
                return dateStore.GetItemByBarcode(ctx.Source.Barcode);
            });

            Field(i => i.Quantity);

            Field(i => i.OrderId);

            Field<OrderType, Order>().Name("Order").ResolveAsync(ctx =>
            {
                return dateStore.GetOrderByIdAsync(ctx.Source.OrderId);
            });

        }
    }
}
