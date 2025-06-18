using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using OnlineShopping.Data;
using OnlineShopping.Models;

namespace OnlineShopping.Controllers
{
    //[Authorize(Roles = "admin")]

    public class ProductController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public ProductController(OnlineShoppingContext context)
        {
            _context = context;
        }

        // GET: Product (Admin Only)
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var onlineShoppingContext = _context.Product.Include(p => p.Category);
            return View(await onlineShoppingContext.ToListAsync());
        }

        // GET: Product Dashboard (For Users and Admins)
        public async Task<IActionResult> ProductDashboard(int? CategoryId)
        {
            // Send categories to the view
            ViewBag.Categories = await _context.Category.ToListAsync();

            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                // Filter products by category
                var productsByCategory = await _context.Product
                    .Where(p => p.CategoryId == CategoryId)
                    .Include(p => p.Category)
                    .ToListAsync();
                return View(productsByCategory);
            }
            else
            {
                // Show all products if no category is selected
                var allProducts = await _context.Product.Include(p => p.Category).ToListAsync();
                return View(allProducts);
            }
        }

        // GET: Product/Details/5
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create (Admin Only)
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // POST: Product/Create (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,CategoryId")] Product product, IFormFile ProductIcon)
        {
            if (ModelState.IsValid)
            {
                // Save the uploaded image if provided
                if (ProductIcon != null)
                {
                    Guid guid = Guid.NewGuid();
                    string fileEx = Path.GetExtension(ProductIcon.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages", guid + fileEx);

                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages"));

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await ProductIcon.CopyToAsync(fileStream);
                    }

                    product.ProductIcon = guid + fileEx;
                }

                // Add the product to the database
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5 (Admin Only)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5 (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,CategoryId,ProductIcon")] Product product, IFormFile ProductIcon)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Save the uploaded image if provided
                    if (ProductIcon != null)
                    {
                        Guid guid = Guid.NewGuid();
                        string fileEx = Path.GetExtension(ProductIcon.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages", guid + fileEx);

                        // Ensure the directory exists
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages"));

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await ProductIcon.CopyToAsync(fileStream);
                        }

                        product.ProductIcon = guid + fileEx;
                    }

                    // Update the product in the database
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5 (Admin Only)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5 (Admin Only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}