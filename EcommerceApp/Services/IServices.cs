using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceApp.Models;
using EcommerceApp.ViewModels;

namespace EcommerceApp.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 12);
        Task<List<Product>> SearchProductsAsync(string searchTerm, int pageNumber = 1, int pageSize = 12);
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> GetProductBySlugAsync(string slug);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 12);
        Task<List<Product>> GetFeaturedProductsAsync();
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<List<Product>> FilterProductsAsync(ProductFilterViewModel filter);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }

    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> GetCategoryBySlugAsync(string slug);
        Task<List<Category>> GetMainCategoriesAsync();
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(int id);
        Task<Order> GetOrderByNumberAsync(string orderNumber);
        Task<List<Order>> GetCustomerOrdersAsync(int customerId);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> CancelOrderAsync(int orderId);
        Task<List<Order>> GetOrdersByStatusAsync(string status);
    }

    public interface ICartService
    {
        Task<Cart> GetCartAsync(string cartId);
        Task<Cart> AddToCartAsync(string cartId, int productId, int quantity);
        Task<Cart> RemoveFromCartAsync(string cartId, int productId);
        Task<Cart> UpdateCartItemQuantityAsync(string cartId, int productId, int quantity);
        Task<bool> ClearCartAsync(string cartId);
        decimal CalculateTotal(Cart cart);
    }

    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<Payment> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment> UpdatePaymentStatusAsync(int paymentId, string status);
        Task<bool> ProcessPaymentAsync(int paymentId);
    }

    public interface IDiscountService
    {
        Task<DiscountCode> GetDiscountCodeAsync(string code);
        Task<decimal> CalculateDiscountAsync(string code, decimal amount);
        Task<DiscountCode> CreateDiscountCodeAsync(DiscountCode discountCode);
        Task<DiscountCode> UpdateDiscountCodeAsync(DiscountCode discountCode);
        Task<bool> DeleteDiscountCodeAsync(int id);
        Task<List<DiscountCode>> GetActiveDiscountsAsync();
    }

    public interface ICustomerService
    {
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<Customer> RegisterCustomerAsync(string email, string password, string firstName, string lastName);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> VerifyPasswordAsync(Customer customer, string password);
        Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(int customerId);
    }

    public interface IReviewService
    {
        Task<List<Review>> GetProductReviewsAsync(int productId);
        Task<Review> CreateReviewAsync(Review review);
        Task<Review> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int id);
        Task<List<Review>> GetCustomerReviewsAsync(int customerId);
        Task UpdateProductRatingAsync(int productId);
    }

    public interface IWishlistService
    {
        Task<List<Wishlist>> GetCustomerWishlistAsync(int customerId);
        Task<Wishlist> AddToWishlistAsync(int customerId, int productId);
        Task<bool> RemoveFromWishlistAsync(int customerId, int productId);
        Task<bool> IsInWishlistAsync(int customerId, int productId);
    }
}
