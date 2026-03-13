using System;
using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Avatar { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Wishlist> WishlistItems { get; set; } = new List<Wishlist>();
        public int LoyaltyPoints { get; set; } = 0;
        public string CustomerGroup { get; set; } = "regular"; // vip, regular, new
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailVerified { get; set; } = false;
        public string VerificationToken { get; set; }
        public string ResetToken { get; set; }
        public DateTime ResetTokenExpiry { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; } = false;
        public string AddressType { get; set; } // "home", "office", "other"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
