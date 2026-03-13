using System;
using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string DetailDescription { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string ThumbnailImage { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<RelatedProduct> RelatedProducts { get; set; } = new List<RelatedProduct>();
        public decimal AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int SalesCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductAttribute
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string AttributeName { get; set; } // e.g., "Size", "Color"
        public string AttributeValue { get; set; } // e.g., "M", "Red"
        public decimal PriceAdjustment { get; set; } = 0;
        public int StockAdjustment { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class RelatedProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int RelatedProductId { get; set; }
        public Product RelatedProductNavigation { get; set; }
        public string RelationType { get; set; } // "similar", "bundle", "alternative"
    }
}
