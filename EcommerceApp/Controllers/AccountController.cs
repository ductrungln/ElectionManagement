using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.ViewModels;
using EcommerceApp.Helpers;

namespace EcommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email và mật khẩu là bắt buộc");
                return View();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null || !PasswordHelper.VerifyPassword(password, customer.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác");
                return View();
            }

            // Set session for customer
            HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
            HttpContext.Session.SetString("CustomerEmail", customer.Email);
            HttpContext.Session.SetString("CustomerName", $"{customer.FirstName} {customer.LastName}");

            // Update last login
            customer.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(
            string email,
            string password,
            string confirmPassword,
            string firstName,
            string lastName)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không trùng khớp");
                return View();
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (existingCustomer != null)
            {
                ModelState.AddModelError("", "Email đã đăng ký");
                return View();
            }

            var customer = new Customer
            {
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: Account/Dashboard
        public IActionResult Dashboard()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // GET: Account/Orders
        public async Task<IActionResult> Orders()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == int.Parse(customerId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var viewModel = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                TrackingNumber = o.TrackingNumber
            }).ToList();

            return View(viewModel);
        }

        // GET: Account/OrderDetail/id
        public async Task<IActionResult> OrderDetail(int id)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == int.Parse(customerId));

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                TrackingNumber = order.TrackingNumber
            };

            return View(viewModel);
        }

        // GET: Account/Addresses
        public async Task<IActionResult> Addresses()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == int.Parse(customerId))
                .ToListAsync();

            return View(addresses);
        }

        // GET: Account/Wishlist
        public async Task<IActionResult> Wishlist()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            var wishlists = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Where(w => w.CustomerId == int.Parse(customerId))
                .ToListAsync();

            var viewModel = wishlists.Select(w => new ProductViewModel
            {
                Id = w.Product.Id,
                Name = w.Product.Name,
                Slug = w.Product.Slug,
                OriginalPrice = w.Product.OriginalPrice,
                SalePrice = w.Product.SalePrice,
                ThumbnailImage = w.Product.ThumbnailImage,
                AverageRating = w.Product.AverageRating,
                ReviewCount = w.Product.ReviewCount,
                Stock = w.Product.Stock
            }).ToList();

            return View(viewModel);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
