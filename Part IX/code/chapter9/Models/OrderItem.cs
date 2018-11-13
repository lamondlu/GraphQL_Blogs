using System.ComponentModel.DataAnnotations.Schema;

namespace chapter1.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public string Barcode { get; set; }

        [ForeignKey("Barcode")]
        public virtual Item Item { get; set; }

        public int Quantity { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
