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
using Newtonsoft.Json;

namespace EcommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_SESSION_KEY = "ShoppingCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            var viewModel = new CartViewModel();

            if (cart.Items.Any())
            {
                viewModel.Items = cart.Items;
                viewModel.SubTotal = cart.Items.Sum(item => item.TotalPrice);
                viewModel.ShippingCost = CalculateShippingCost(viewModel.SubTotal);
                viewModel.TotalAmount = viewModel.SubTotal + viewModel.ShippingCost;
            }

            return View(viewModel);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string attributes = "")
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.Stock < quantity)
            {
                return Json(new { success = false, message = "Số lượng vượt quá tồn kho" });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                var newItem = new CartItemViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    ProductImage = product.ThumbnailImage ?? product.Images.FirstOrDefault()?.ImageUrl,
                    Quantity = quantity,
                    UnitPrice = product.SalePrice,
                    TotalPrice = product.SalePrice * quantity,
                    SelectedAttributes = string.IsNullOrEmpty(attributes) ? 
                        new Dictionary<string, string>() : 
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(attributes)
                };

                cart.Items.Add(newItem);
            }

            SaveCartToSession(cart);

            return Json(new { 
                success = true, 
                message = "Đã thêm vào giỏ hàng",
                cartCount = cart.Items.Count
            });
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCartToSession(cart);
            }

            return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveItem(productId);
            }

            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                item.TotalPrice = item.UnitPrice * quantity;
                SaveCartToSession(cart);
            }

            return Json(new { success = true, message = "Cập nhật thành công" });
        }

        // POST: Cart/ApplyCoupon
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var discount = await _context.DiscountCodes
                .FirstOrDefaultAsync(d => d.Code == couponCode && d.IsActive && 
                    d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);

            if (discount == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ" });
            }

            var cart = GetCartFromSession();
            var subtotal = cart.Items.Sum(item => item.TotalPrice);

            if (subtotal < discount.MinOrderAmount)
            {
                return Json(new { success = false, message = $"Đơn hàng tối thiểu {discount.MinOrderAmount}" });
            }

            decimal discountAmount = 0;
            if (discount.DiscountType == "percentage")
            {
                discountAmount = subtotal * discount.DiscountValue / 100;
                if (discount.MaxDiscountAmount > 0 && discountAmount > discount.MaxDiscountAmount)
                {
                    discountAmount = discount.MaxDiscountAmount;
                }
            }
            else
            {
                discountAmount = discount.DiscountValue;
            }

            HttpContext.Session.SetString("AppliedCoupon", couponCode);
            HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());

            return Json(new { 
                success = true, 
                message = "Áp dụng mã giảm giá thành công",
                discountAmount = discountAmount
            });
        }

        // POST: Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            HttpContext.Session.Remove("AppliedCoupon");
            HttpContext.Session.Remove("DiscountAmount");

            return Json(new { success = true, message = "Giỏ hàng đã được làm trống" });
        }

        private CartModel GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new CartModel();
            }

            return JsonConvert.DeserializeObject<CartModel>(sessionData) ?? new CartModel();
        }

        private void SaveCartToSession(CartModel cart)
        {
            HttpContext.Session.SetString(CART_SESSION_KEY, JsonConvert.SerializeObject(cart));
        }

        private decimal CalculateShippingCost(decimal subtotal)
        {
            // Simple shipping cost calculation
            if (subtotal >= 500000)
                return 0; // Free shipping
            else if (subtotal >= 200000)
                return 25000;
            else
                return 40000;
        }
    }

    public class CartModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    }
}
