using System;

namespace EcommerceApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string PaymentMethod { get; set; } // cod, bank_transfer, wallet, online_gateway
        public string PaymentGateway { get; set; } // stripe, paypal, momo, zalopay, etc.
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "pending"; // pending, processing, completed, failed, refunded
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime PaidAt { get; set; }
        public string FailureReason { get; set; }
        public string ResponseData { get; set; } // JSON response from gateway
    }
}
