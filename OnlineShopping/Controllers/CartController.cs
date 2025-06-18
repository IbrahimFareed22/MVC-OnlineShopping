using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShopping.Models;
using System.Linq;


namespace OnlineShopping.Controllers
{
    [Authorize] 
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserIdFromSession()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            return string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);
        }


        // GET: Cart
        public IActionResult Index()
        {
            int userId = GetUserIdFromSession();
            var cart = _cartService.GetCart(userId); // Guaranteed not null
            var cartList = new List<Cart> { cart };

            ViewBag.TotalPrice = _cartService.GetTotal(userId);
            return View(cartList);
        }


        // POST: Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, string productName, decimal price, int quantity = 1)
        {
            int userId = GetUserIdFromSession();
            var item = new CartItem
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };

            _cartService.AddToCart(userId, item);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            int userId = GetUserIdFromSession();
            _cartService.RemoveFromCart(userId, productId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public IActionResult Checkout()
        {
            int userId = GetUserIdFromSession();
            var total = _cartService.GetTotal(userId);

           
            _cartService.ClearCart(userId);

            ViewBag.Total = total;
            return View("Checkout");
        }
    }
}
