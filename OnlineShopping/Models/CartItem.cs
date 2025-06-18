using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShopping.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }  // لازم مفتاح أساسي

        [ForeignKey("Cart")]
        public int CartId { get; set; }  // مفتاح خارجي

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public Cart Cart { get; set; }  // Navigation Property
    }
}
