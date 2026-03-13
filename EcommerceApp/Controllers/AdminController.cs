using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Helpers;

namespace EcommerceApp.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            // TODO: Implement proper authentication/authorization
            return true;
        }

        // GET: admin/dashboard
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return Unauthorized();

            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == "paid")
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewData["TotalProducts"] = totalProducts;
            ViewData["TotalCustomers"] = totalCustomers;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["RecentOrders"] = recentOrders;

            return View();
        }

        // GET: admin/products
        [Route("products")]
        public async Task<IActionResult> Products(int pageNumber = 1, int pageSize = 20)
        {
            if (!IsAdmin()) return Unauthorized();

            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await _context.Products.CountAsync();
            ViewData["TotalPages"] = (totalItems + pageSize - 1) / pageSize;
            ViewData["CurrentPage"] = pageNumber;

            return View(products);
        }

        // GET: admin/products/create
        [Route("products/create")]
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!IsAdmin()) return Unauthorized();

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewData["Categories"] = categories;

            return View();
        }

        // POST: admin/products/create
        [Route("products/create")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!IsAdmin()) return Unauthorized();

            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                ViewData["Categories"] = categories;
                return View(product);
            }

            product.Slug = SlugHelper.GenerateSlug(product.Name);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        // GET: admin/products/edit/5
        [Route("products/edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var product = await _context.Products
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewData["Categories"] = categories;

            return View(product);
        }

        // POST: admin/products/edit/5
        [Route("products/edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (!IsAdmin()) return Unauthorized();

            if (id != product.Id) return NotFound();

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.DetailDescription = product.DetailDescription;
            existingProduct.OriginalPrice = product.OriginalPrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.Stock = product.Stock;
            existingProduct.Brand = product.Brand;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ThumbnailImage = product.ThumbnailImage;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        // POST: admin/products/delete/5
        [Route("products/delete/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Sản phẩm đã được xóa" });
        }

        // GET: admin/categories
        [Route("categories")]
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return Unauthorized();

            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();

            return View(categories);
        }

        // POST: admin/categories/create
        [Route("categories/create")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!IsAdmin()) return Unauthorized();

            category.Slug = SlugHelper.GenerateSlug(category.Name);
            category.CreatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Categories");
        }

        // GET: admin/orders
        [Route("orders")]
        public async Task<IActionResult> Orders(string status = null, int pageNumber = 1, int pageSize = 20)
        {
            if (!IsAdmin()) return Unauthorized();

            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = (totalItems + pageSize - 1) / pageSize;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["Status"] = status;

            return View(orders);
        }

        // GET: admin/orders/detail/5
        [Route("orders/detail/{id}")]
        public async Task<IActionResult> OrderDetail(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: admin/orders/update-status
        [Route("orders/update-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin()) return Unauthorized();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == "delivered")
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Trạng thái đã được cập nhật" });
        }

        // GET: admin/discounts
        [Route("discounts")]
        public async Task<IActionResult> Discounts()
        {
            if (!IsAdmin()) return Unauthorized();

            var discounts = await _context.DiscountCodes
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(discounts);
        }

        // POST: admin/discounts/create
        [Route("discounts/create")]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount(DiscountCode discount)
        {
            if (!IsAdmin()) return Unauthorized();

            if (discount.StartDate < DateTime.UtcNow)
            {
                discount.StartDate = DateTime.UtcNow;
            }

            _context.DiscountCodes.Add(discount);
            await _context.SaveChangesAsync();

            return RedirectToAction("Discounts");
        }

        // POST: admin/discounts/delete/5
        [Route("discounts/delete/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var discount = await _context.DiscountCodes.FindAsync(id);
            if (discount == null) return NotFound();

            _context.DiscountCodes.Remove(discount);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Mã giảm giá đã được xóa" });
        }
    }
}
