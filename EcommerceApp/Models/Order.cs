using System;
using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingDistrict { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingDistrict { get; set; }
        public string BillingWard { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "pending"; // pending, confirmed, processing, shipping, delivered, cancelled
        public string PaymentStatus { get; set; } = "unpaid"; // unpaid, paid, refunded
        public string PaymentMethod { get; set; } // cod, bank_transfer, wallet, online_gateway
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public string CustomerNote { get; set; }
        public string AdminNote { get; set; }
        public Payment Payment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ShippedAt { get; set; }
        public DateTime DeliveredAt { get; set; }
        public DateTime CancelledAt { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalPrice { get; set; }
        public string SelectedAttributes { get; set; } // JSON string of selected attributes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
