using GraphQL.Types;

namespace chapter1
{
    public class OrderInputType : InputObjectGraphType
    {
        public OrderInputType()
        {
            Name = "OrderInput";
            Field<NonNullGraphType<StringGraphType>>("tag");
            Field<NonNullGraphType<DateGraphType>>("createdAt");
            Field<NonNullGraphType<IntGraphType>>("customerId");
        }
    }
}
