using OnlineShopping.Models;

public class CartService
{
    private static Dictionary<int, Cart> carts = new Dictionary<int, Cart>();

    public Cart GetCart(int userId)
    {
        if (!carts.ContainsKey(userId))
        {
            carts[userId] = new Cart { UserId = userId, Items = new List<CartItem>() }; // Initialize Items list here
        }

        return carts[userId];
    }

    public void AddToCart(int userId, CartItem item)
    {
        var cart = GetCart(userId); // Ensure that cart.Items is initialized here
        var existing = cart.Items.FirstOrDefault(p => p.ProductId == item.ProductId);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }
    }

    public void RemoveFromCart(int userId, int productId)
    {
        var cart = GetCart(userId);
        var item = cart.Items.FirstOrDefault(p => p.ProductId == productId);
        if (item != null)
            cart.Items.Remove(item);
    }

    public void ClearCart(int userId)
    {
        if (carts.ContainsKey(userId))
            carts[userId].Items.Clear();
    }

    public decimal GetTotal(int userId)
    {
        return GetCart(userId).Items.Sum(x => x.Price * x.Quantity);
    }
}
