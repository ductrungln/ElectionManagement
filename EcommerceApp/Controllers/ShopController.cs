using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.ViewModels;
using EcommerceApp.Helpers;

namespace EcommerceApp.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shop
        public async Task<IActionResult> Index(
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy = "newest",
            int pageNumber = 1,
            int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive);

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice <= maxPrice.Value);
            }

            // Apply sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.SalePrice),
                "price_desc" => query.OrderByDescending(p => p.SalePrice),
                "best_selling" => query.OrderByDescending(p => p.SalesCount),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Pagination
            var skip = (pageNumber - 1) * pageSize;
            var totalItems = await query.CountAsync();
            var products = await query.Skip(skip).Take(pageSize).ToListAsync();

            var viewModel = new ProductFilterViewModel
            {
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewData["TotalPages"] = PaginationHelper.CalculateTotalPages(totalItems, pageSize);
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalItems"] = totalItems;
            ViewData["Products"] = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                OriginalPrice = p.OriginalPrice,
                SalePrice = p.SalePrice,
                ThumbnailImage = p.ThumbnailImage,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                Stock = p.Stock
            }).ToList();

            return View(viewModel);
        }

        // GET: Shop/Details/5
        public async Task<IActionResult> Details(string slug)
        {
            if (slug == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.RelatedProducts)
                    .ThenInclude(rp => rp.RelatedProductNavigation)
                .FirstOrDefaultAsync(m => m.Slug == slug && m.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // Update view count
            product.ViewCount++;
            await _context.SaveChangesAsync();

            var viewModel = new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                DetailDescription = product.DetailDescription,
                OriginalPrice = product.OriginalPrice,
                SalePrice = product.SalePrice,
                Stock = product.Stock,
                Brand = product.Brand,
                Images = product.Images.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    AltText = img.AltText,
                    DisplayOrder = img.DisplayOrder
                }).ToList(),
                Attributes = product.Attributes.Select(attr => new ProductAttributeViewModel
                {
                    Id = attr.Id,
                    AttributeName = attr.AttributeName,
                    AttributeValue = attr.AttributeValue,
                    PriceAdjustment = attr.PriceAdjustment
                }).ToList(),
                Reviews = product.Reviews
                    .Where(r => r.IsApproved)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        CustomerName = r.Customer.FirstName,
                        Rating = r.Rating,
                        Title = r.Title,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        AdminResponse = r.AdminResponse
                    }).ToList(),
                RelatedProducts = product.RelatedProducts.Take(5).Select(rp => new ProductViewModel
                {
                    Id = rp.RelatedProductNavigation.Id,
                    Name = rp.RelatedProductNavigation.Name,
                    Slug = rp.RelatedProductNavigation.Slug,
                    OriginalPrice = rp.RelatedProductNavigation.OriginalPrice,
                    SalePrice = rp.RelatedProductNavigation.SalePrice,
                    ThumbnailImage = rp.RelatedProductNavigation.ThumbnailImage,
                    AverageRating = rp.RelatedProductNavigation.AverageRating,
                    ReviewCount = rp.RelatedProductNavigation.ReviewCount
                }).ToList(),
                AverageRating = product.AverageRating,
                ReviewCount = product.ReviewCount
            };

            return View(viewModel);
        }

        // GET: Shop/Search
        public async Task<IActionResult> Search(string q, int pageNumber = 1, int pageSize = 12)
        {
            if (string.IsNullOrEmpty(q))
            {
                return RedirectToAction("Index");
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive && (
                    p.Name.Contains(q) ||
                    p.Description.Contains(q) ||
                    p.Brand.Contains(q)
                ))
                .OrderByDescending(p => p.CreatedAt);

            var totalItems = await query.CountAsync();
            var skip = (pageNumber - 1) * pageSize;
            var products = await query.Skip(skip).Take(pageSize).ToListAsync();

            ViewData["SearchTerm"] = q;
            ViewData["TotalPages"] = PaginationHelper.CalculateTotalPages(totalItems, pageSize);
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalItems"] = totalItems;
            ViewData["Products"] = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                OriginalPrice = p.OriginalPrice,
                SalePrice = p.SalePrice,
                ThumbnailImage = p.ThumbnailImage,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                Stock = p.Stock
            }).ToList();

            return View("Index");
        }

        // GET: Shop/Category/slug
        public async Task<IActionResult> Category(string slug, int pageNumber = 1, int pageSize = 12)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == category.Id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt);

            var totalItems = await query.CountAsync();
            var skip = (pageNumber - 1) * pageSize;
            var products = await query.Skip(skip).Take(pageSize).ToListAsync();

            ViewData["CategoryName"] = category.Name;
            ViewData["TotalPages"] = PaginationHelper.CalculateTotalPages(totalItems, pageSize);
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalItems"] = totalItems;
            ViewData["Products"] = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                OriginalPrice = p.OriginalPrice,
                SalePrice = p.SalePrice,
                ThumbnailImage = p.ThumbnailImage,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                Stock = p.Stock
            }).ToList();

            return View("Index");
        }
    }
}
