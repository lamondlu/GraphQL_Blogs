using GraphQL.Types;

namespace chapter1
{
    public class InventorySchema : Schema
    {
        public InventorySchema(InventoryQuery query, InventoryMutation mutation)
        {
            Query = query;
            Mutation = mutation;
        }
    }
}
