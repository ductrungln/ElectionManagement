using System;

namespace EcommerceApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public string Title { get; set; }
        public string Comment { get; set; }
        public int HelpfulCount { get; set; } = 0;
        public int UnhelpfulCount { get; set; } = 0;
        public bool IsVerifiedPurchase { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public string AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
