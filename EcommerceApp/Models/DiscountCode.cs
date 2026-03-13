using System;
using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class DiscountCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; } // "percentage", "fixed"
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; } = 0;
        public int UsagePerCustomer { get; set; } = 1;
        public string ApplicableProductIds { get; set; } // JSON serialized list: "[1,2,3]"
        public string ApplicableCategoryIds { get; set; } // JSON serialized list: "[1,2,3]"
        public bool ApplyToAll { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
