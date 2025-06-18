
using System.ComponentModel.DataAnnotations;

namespace OnlineShopping.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(100, ErrorMessage = "The category name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        public virtual IList<Product>? Products { get; set; }
    }
}
