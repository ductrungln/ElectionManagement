using System;
using System.Collections.Generic;

namespace EcommerceApp.ViewModels
{
    public class ProductFilterViewModel
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Brand { get; set; }
        public string SortBy { get; set; } // price_asc, price_desc, newest, best_selling, rating
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string SearchTerm { get; set; }
        public List<string> SelectedSizes { get; set; } = new List<string>();
        public List<string> SelectedColors { get; set; } = new List<string>();
        public decimal? MinRating { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AppliedCoupon { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Dictionary<string, string> SelectedAttributes { get; set; } = new Dictionary<string, string>();
    }

    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DetailDescription { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public string Brand { get; set; }
        public List<ProductImageViewModel> Images { get; set; } = new List<ProductImageViewModel>();
        public List<ProductAttributeViewModel> Attributes { get; set; } = new List<ProductAttributeViewModel>();
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public List<ProductViewModel> RelatedProducts { get; set; } = new List<ProductViewModel>();
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string ThumbnailImage { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int Stock { get; set; }
        public decimal DiscountPercentage => OriginalPrice > 0 ? ((OriginalPrice - SalePrice) / OriginalPrice * 100) : 0;
    }

    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProductAttributeViewModel
    {
        public int Id { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public decimal PriceAdjustment { get; set; }
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AdminResponse { get; set; }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string TrackingNumber { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        }
    }
}
