using System.ComponentModel.DataAnnotations;

namespace OnlineShopping.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        public List<CartItem> Items { get; set; } = new();
    }
}
