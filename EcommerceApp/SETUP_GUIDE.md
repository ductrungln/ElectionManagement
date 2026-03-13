# EcommerceApp - Quick Start Guide

## Project Overview

EcommerceApp is a comprehensive e-commerce platform built with ASP.NET Core 5.0, featuring a complete e-commerce ecosystem with:
- Product catalog and category management
- Shopping cart and checkout system
- Customer account management
- Admin dashboard for store management
- Order and payment processing
- Discount and promotion management
- Customer reviews and ratings
- Wishlist functionality

## Project Structure

```
EcommerceApp/
├── Controllers/
│   ├── HomeController.cs          # Homepage and main navigation
│   ├── ShopController.cs          # Product browsing and search
│   ├── CartController.cs          # Shopping cart operations
│   ├── AccountController.cs       # Customer account functions
│   └── AdminController.cs         # Admin panel functions
├── Models/                        # Entity models
│   ├── Category.cs
│   ├── Product.cs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Review.cs
│   ├── Wishlist.cs
│   ├── DiscountCode.cs
│   ├── Payment.cs
│   └── Address.cs
├── Data/
│   └── ApplicationDbContext.cs    # Entity Framework DbContext
├── Services/
│   └── IServices.cs               # Service interface definitions
├── ViewModels/                    # Data transfer objects
│   ├── ViewModels.cs
│   └── Cart.cs
├── Helpers/                       # Utility classes
│   ├── Helpers.cs
│   ├── Constants.cs
│   └── ApiResponse.cs
├── Pages/                         # Razor Pages
│   ├── Shop/
│   ├── Cart/
│   ├── Account/
│   ├── Admin/
│   └── Shared/
├── wwwroot/                       # Static files
│   ├── css/
│   ├── js/
│   └── images/
├── Migrations/                    # EF Core migrations
├── appsettings.json              # Application configuration
├── appsettings.Development.json  # Development configuration
├── Program.cs                     # Entry point
├── Startup.cs                     # Startup configuration
├── EcommerceApp.csproj           # Project file
└── README.md                      # Documentation

```

## Prerequisites

- .NET 5.0 SDK or later
- SQL Server LocalDB or SQL Server Express
- Visual Studio Code, Visual Studio 2019+, or any C# IDE
- Git (for version control)

## Installation Steps

### 1. Clone the Repository
```bash
git clone <repository-url>
cd EcommerceApp
```

### 2. Restore NuGet Packages
```bash
dotnet restore
```

### 3. Database Configuration
The project uses SQL Server LocalDB by default. The connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceAppDb;Trusted_Connection=true;"
  }
}
```

To use a different SQL Server instance, update the connection string:
```json
"DefaultConnection": "Server=<your-server-name>;Database=EcommerceAppDb;User Id=<username>;Password=<password>;"
```

### 4. Apply Database Migrations
```bash
dotnet ef database update
```

This command will:
- Create the EcommerceAppDb database
- Create all required tables
- Set up relationships between tables
- Create indexes for performance

### 5. Build the Project
```bash
dotnet build
```

### 6. Run the Application
```bash
dotnet run
```

The application will start at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## Database Tables

The application creates the following tables:

### Core Tables
- **Categories** - Product categories with parent-child relationships
- **Products** - Product catalog with detailed information
- **ProductImages** - Multiple images per product
- **ProductAttributes** - Product variants (size, color, etc.)
- **RelatedProducts** - Product recommendations

### Customer Tables
- **Customers** - Customer accounts and profiles
- **Addresses** - Customer shipping and billing addresses
- **Wishlists** - Customer favorite products

### Order Tables
- **Orders** - Customer orders
- **OrderItems** - Individual items in orders
- **Payments** - Payment transaction records

### Feedback Tables
- **Reviews** - Customer product reviews and ratings

### Promotion Tables
- **DiscountCodes** - Coupon and promotional codes

## Key Features by Module

### Shop Module (ShopController)
- Product listing with pagination
- Product search and filtering
- Advanced sorting options
- Category browsing
- Product detail pages
- View count tracking
- Related products display

### Cart Module (CartController)
- Add/remove products
- Update quantities
- Apply discount codes
- Calculate shipping automatically
- Session-based persistence
- Cart subtotal and total calculation

### Account Module (AccountController)
- User registration
- Login/logout
- Password management
- Order history
- Address management
- Wishlist management
- Customer dashboard

### Admin Module (AdminController)
- Dashboard with analytics
- Product management (CRUD)
- Category management
- Order management and tracking
- Discount code management
- Order status updates
- Report generation

## API Endpoints

### Shop Endpoints
```
GET  /shop                          List all products
GET  /shop/details/{slug}           Product details
GET  /shop/category/{slug}          Products by category
GET  /shop/search?q={query}         Search products
```

### Cart Endpoints
```
GET  /cart                          View cart
POST /cart/add-to-cart              Add item
POST /cart/remove-item              Remove item
POST /cart/update-quantity          Update quantity
POST /cart/apply-coupon             Apply discount
POST /cart/clear                    Clear cart
```

### Account Endpoints
```
GET  /account/login                 Login page
POST /account/login                 Login
GET  /account/register              Register page
POST /account/register              Register
GET  /account/dashboard             Dashboard
GET  /account/orders                Order history
GET  /account/wishlist              Wishlist
GET  /account/logout                Logout
```

### Admin Endpoints
```
GET  /admin/dashboard               Dashboard
GET  /admin/products                Product list
POST /admin/products/create         Create product
POST /admin/products/edit/{id}      Edit product
POST /admin/products/delete/{id}    Delete product
GET  /admin/orders                  Order management
POST /admin/orders/update-status    Update order
GET  /admin/discounts               Discount list
POST /admin/discounts/create        Create discount
GET  /admin/categories              Category list
```

## Helper Classes

### PasswordHelper
- `HashPassword(string password)` - Hash passwords securely
- `VerifyPassword(string password, string hash)` - Verify passwords

### SlugHelper
- `GenerateSlug(string text)` - Generate URL-friendly slugs
- `RemoveDiacritics(string text)` - Remove Vietnamese accents

### PaginationHelper
- `CalculateTotalPages(int total, int pageSize)` - Calculate pages
- `CalculateSkip(int page, int pageSize)` - Calculate skip count

### FileHelper
- `GenerateFileName(string originalName)` - Generate unique file names
- `IsValidImageFile(string fileName)` - Validate image files

## Configuration Files

### appsettings.json
Main configuration file:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceAppDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### launchSettings.json
Application launch profiles:
- IIS Express configuration
- Development server settings
- HTTPS configuration

## Common Tasks

### Creating a New Controller
```csharp
public class MyController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public MyController(ApplicationDbContext context)
    {
        _context = context;
    }
}
```

### Adding a New Model
1. Create class in `Models/` folder
2. Add DbSet to `ApplicationDbContext`
3. Create migration: `dotnet ef migrations add ModelName`
4. Apply migration: `dotnet ef database update`

### Adding a New Service
1. Define interface in `Services/IServices.cs`
2. Create implementation in `Services/`
3. Register in `Startup.cs`: `services.AddScoped<IService, ServiceImpl>();`

### Running Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigration

# List migrations
dotnet ef migrations list

# Remove last migration
dotnet ef migrations remove
```

## Troubleshooting

### Database Connection Issues
1. Check connection string in `appsettings.json`
2. Verify SQL Server is running
3. Ensure LocalDB is installed: `sqllocaldb info`
4. Use SQL Server Management Studio to test connection

### Build Errors
1. Run `dotnet clean` to clean build artifacts
2. Run `dotnet restore` to restore packages
3. Check `.csproj` file for correct package versions

### Migration Issues
1. Check database state: `dotnet ef database info`
2. List pending migrations: `dotnet ef migrations list`
3. Remove last migration if needed: `dotnet ef migrations remove`
4. Create new migration if schema changed

### Port Already in Use
If port 5001 is in use, modify `launchSettings.json`:
```json
"applicationUrl": "https://localhost:<new-port>";
```

## Next Steps

1. **Customize Branding**
   - Update logo and colors in wwwroot/css/site.css
   - Modify page layouts in Pages/Shared/

2. **Add Content**
   - Create categories in admin panel
   - Add products with images
   - Set up discount codes

3. **Implement Payment Gateway**
   - Integrate Stripe, PayPal, or other gateway
   - Update Payment model and PaymentController

4. **Add Email Notifications**
   - Configure SMTP settings
   - Create email templates
   - Implement email service

5. **Performance Optimization**
   - Enable output caching
   - Optimize database queries
   - Implement pagination
   - Add search indexing

## Support & Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)

## Security Checklist

- [ ] Change default connection string
- [ ] Implement proper authentication
- [ ] Add role-based authorization
- [ ] Enable HTTPS
- [ ] Configure CORS properly
- [ ] Validate all user inputs
- [ ] Implement CSRF protection
- [ ] Use parameterized queries
- [ ] Hash passwords securely
- [ ] Implement rate limiting
- [ ] Regular security updates

## Deploy to Production

1. Build release version: `dotnet publish -c Release`
2. Configure production connection string
3. Set up SSL certificate
4. Configure IIS or Azure App Service
5. Run migrations on production database
6. Monitor application performance
7. Set up automated backups

## License

This project is provided as-is for educational and commercial purposes.

## Support

For questions and issues, please refer to the README.md file or create an issue on the repository.
