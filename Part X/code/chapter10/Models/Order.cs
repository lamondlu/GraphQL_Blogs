using chapter1.Models;
using System;
using System.Collections.Generic;

namespace chapter1
{
    public class Order
    {
        public int OrderId { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }

        public Customer Customer { get; set; }
        public int CustomerId { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
