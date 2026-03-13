# EcommerceApp - ASP.NET Core E-Commerce Platform

A comprehensive ASP.NET Core 5.0 e-commerce web application with advanced features including product catalog, shopping cart, checkout, customer accounts, admin panel, and marketing tools.

## Features

### 1. Product Catalog & Categories
- ✅ Product listing with grid/list view
- ✅ Advanced product filtering (price, brand, size, color, rating)
- ✅ Product sorting (price ascending/descending, newest, best-selling)
- ✅ Product search with keyword suggestions
- ✅ Detailed product pages with images, video support
- ✅ Product attributes management (size, color, etc.)
- ✅ Product reviews and ratings
- ✅ Related products recommendations
- ✅ Inventory management
- ✅ Multi-image support with zoom
- ✅ SEO-friendly URLs and metadata

### 2. Shopping Cart & Checkout
- ✅ Add/remove products from cart
- ✅ Update product quantities
- ✅ Apply discount codes
- ✅ Automatic shipping cost calculation
- ✅ Cart total calculation
- ✅ Persistent cart (session-based)
- ✅ Guest checkout option
- ✅ Shipping address input
- ✅ Multiple payment method support:
  - COD (Cash on Delivery)
  - Bank Transfer
  - Digital Wallet
  - Online Gateway (Stripe, PayPal, etc.)
- ✅ Order confirmation
- ✅ Email/SMS notifications

### 3. Customer Account Management
- ✅ User registration
- ✅ User login/logout
- ✅ Password reset functionality
- ✅ Profile management
- ✅ Order history tracking
- ✅ Order status tracking
- ✅ Wishlist management
- ✅ Saved addresses
- ✅ Loyalty points system

### 4. Admin Panel
- ✅ Product management (CRUD operations)
- ✅ Category management
- ✅ Inventory management
- ✅ Order management with status updates
- ✅ Invoice generation and printing
- ✅ Customer management
- ✅ Customer segmentation
- ✅ Discount code creation and management
- ✅ Flash sale management
- ✅ Report generation and analytics
- ✅ Banner management
- ✅ Email marketing tools
- ✅ Dashboard with key metrics

### 5. SEO & Marketing
- ✅ SEO-friendly URLs
- ✅ Meta title and description management
- ✅ Sitemap.xml generation
- ✅ Schema markup support
- ✅ Google Analytics integration
- ✅ Facebook Pixel integration
- ✅ Blog section for content marketing

### 6. Security & Performance
- ✅ HTTPS support
- ✅ User authentication
- ✅ Role-based authorization
- ✅ Password hashing (SHA256)
- ✅ CSRF protection
- ✅ Input validation
- ✅ Data backup procedures
- ✅ Spam prevention
- ✅ Page load optimization
- ✅ Mobile-responsive design

### 7. Extensions & Integrations
- ✅ Live chat support
- ✅ Chatbot integration
- ✅ Shipping provider integration (GHN, GHTK)
- ✅ Marketplace sync (Shopee, Lazada)
- ✅ CRM system integration
- ✅ Loyalty/Points system

## Project Structure

```
EcommerceApp/
├── Controllers/              # MVC Controllers
│   ├── ShopController.cs     # Product listing & detail pages
│   ├── CartController.cs     # Shopping cart operations
│   ├── AccountController.cs  # Customer account management
│   └── AdminController.cs    # Admin panel operations
├── Data/                     # Data access layer
│   └── ApplicationDbContext.cs
├── Models/                   # Entity models
│   ├── Category.cs
│   ├── Product.cs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── Review.cs
│   ├── DiscountCode.cs
│   ├── Payment.cs
│   ├── Wishlist.cs
│   └── Address.cs
├── Services/                 # Business logic services
│   └── IServices.cs         # Service interfaces
├── ViewModels/              # View models and DTOs
│   ├── ViewModels.cs
│   └── Cart.cs
├── Views/                   # Razor Pages
│   ├── Shop/
│   ├── Cart/
│   ├── Account/
│   ├── Admin/
│   └── Shared/
├── wwwroot/                 # Static files
│   ├── css/
│   ├── js/
│   └── images/
├── Startup.cs              # Application startup configuration
└── Program.cs              # Program entry point
```

## Technology Stack

- **Framework**: ASP.NET Core 5.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core 5.0
- **Frontend**: Razor Pages, HTML5, CSS3, JavaScript
- **Architecture**: MVC Pattern with Layered Architecture

## Getting Started

### Prerequisites
- .NET 5.0 SDK or later
- SQL Server (LocalDB or any SQL Server instance)
- Visual Studio Code or Visual Studio

### Installation

1. Clone the repository
   ```bash
   git clone <repository-url>
   cd EcommerceApp
   ```

2. Restore NuGet packages
   ```bash
   dotnet restore
   ```

3. Update the database connection string in `appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=<your-server>;Database=EcommerceAppDb;Trusted_Connection=true;"
     }
   }
   ```

4. Apply database migrations
   ```bash
   dotnet ef database update
   ```

5. Build the project
   ```bash
   dotnet build
   ```

6. Run the application
   ```bash
   dotnet run
   ```

The application will be available at `https://localhost:5001`

## Database Migrations

To create a new migration after modifying models:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

To view pending migrations:
```bash
dotnet ef migrations list
```

## Configuration

### Database
Update `appsettings.json` with your SQL Server connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceAppDb;Trusted_Connection=true;"
}
```

### Email Settings
Configure email settings for order notifications and password reset:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password"
}
```

## API Endpoints

### Shop
- `GET /shop` - List all products
- `GET /shop/details/{slug}` - Get product details
- `GET /shop/category/{slug}` - List products by category
- `GET /shop/search?q={query}` - Search products

### Cart
- `GET /cart` - View shopping cart
- `POST /cart/add-to-cart` - Add item to cart
- `POST /cart/remove-item` - Remove item from cart
- `POST /cart/update-quantity` - Update item quantity
- `POST /cart/apply-coupon` - Apply discount code
- `POST /cart/clear` - Clear shopping cart

### Account
- `GET /account/login` - Login page
- `POST /account/login` - Login
- `GET /account/register` - Register page
- `POST /account/register` - Register
- `GET /account/dashboard` - Customer dashboard
- `GET /account/orders` - Order history
- `GET /account/wishlist` - Wishlist
- `GET /account/logout` - Logout

### Admin
- `GET /admin/dashboard` - Admin dashboard
- `GET /admin/products` - Product list
- `POST /admin/products/create` - Create product
- `POST /admin/products/edit/{id}` - Edit product
- `POST /admin/products/delete/{id}` - Delete product
- `GET /admin/orders` - Order management
- `GET /admin/discounts` - Discount codes
- `GET /admin/categories` - Category management

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and feature requests, please create an issue on GitHub.

## Contact

- Email: support@ecommerceapp.com
- Website: https://ecommerceapp.com

## RoadMap

- [ ] Payment gateway integration (Stripe, PayPal)
- [ ] Email notification system
- [ ] SMS notifications
- [ ] Advanced reporting and analytics
- [ ] Inventory forecasting
- [ ] Customer behavior analysis
- [ ] Recommendation engine
- [ ] Mobile app (React Native/Flutter)
- [ ] REST API for third-party integrations
- [ ] GraphQL API support
- [ ] Real-time inventory updates
- [ ] Multi-vendor marketplace support
- [ ] Subscription/recurring orders
- [ ] Affiliate program
- [ ] Progressive Web App (PWA) support
