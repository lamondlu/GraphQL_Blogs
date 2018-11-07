using Newtonsoft.Json.Linq;

namespace chapter1
{
    public class GraphQLRequest
    {
        public string Query { get; set; }

        public JObject Variables { get; set; }
    }
}
