# EcommerceApp API Documentation

## Overview

This document describes all available API endpoints in the EcommerceApp. The API is built following RESTful principles and returns JSON responses.

## Base URL

```
http://localhost:5000
https://localhost:5001
```

## Response Format

All API responses follow a standard format:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* response data */ }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Error 1", "Error 2"]
}
```

## Authentication

Currently, the application uses session-based authentication. Include session cookies in requests automatically handled by the browser.

Future versions will support:
- JWT Token authentication
- API key authentication
- OAuth 2.0

## Endpoints by Module

---

## SHOP API

### List All Products

**GET** `/shop`

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 12) - Items per page
- `categoryId` (int, optional) - Filter by category
- `minPrice` (decimal, optional) - Minimum price filter
- `maxPrice` (decimal, optional) - Maximum price filter
- `sortBy` (string, optional) - Sort option
  - `newest` - Recently added
  - `price_asc` - Price ascending
  - `price_desc` - Price descending
  - `best_selling` - Best sellers
  - `rating` - Customer rating

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Product Name",
      "slug": "product-name",
      "originalPrice": 1000000,
      "salePrice": 800000,
      "thumbnailImage": "/images/product.jpg",
      "averageRating": 4.5,
      "reviewCount": 12,
      "stock": 50
    }
  ]
}
```

**Example:**
```bash
GET /shop?pageNumber=1&pageSize=12&categoryId=1&sortBy=price_asc
```

---

### Get Product Details

**GET** `/shop/details/{slug}`

**Path Parameters:**
- `slug` (string, required) - Product URL slug

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Product Name",
    "slug": "product-name",
    "description": "Short description",
    "detailDescription": "Detailed description",
    "originalPrice": 1000000,
    "salePrice": 800000,
    "stock": 50,
    "brand": "Brand Name",
    "images": [
      {
        "id": 1,
        "imageUrl": "/images/product1.jpg",
        "altText": "Product image",
        "displayOrder": 1
      }
    ],
    "attributes": [
      {
        "id": 1,
        "attributeName": "Size",
        "attributeValue": "M",
        "priceAdjustment": 0
      }
    ],
    "reviews": [
      {
        "id": 1,
        "customerName": "John",
        "rating": 5,
        "title": "Excellent product",
        "comment": "Very satisfied",
        "createdAt": "2024-03-03T10:30:00Z"
      }
    ],
    "relatedProducts": [
      {
        "id": 2,
        "name": "Related Product",
        "slug": "related-product",
        "originalPrice": 1000000,
        "salePrice": 800000,
        "thumbnailImage": "/images/related.jpg"
      }
    ],
    "averageRating": 4.5,
    "reviewCount": 2
  }
}
```

**Example:**
```bash
GET /shop/details/awesome-product-name
```

---

### Search Products

**GET** `/shop/search`

**Query Parameters:**
- `q` (string, required) - Search term
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 12) - Items per page

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Product Name",
      "slug": "product-name",
      "originalPrice": 1000000,
      "salePrice": 800000,
      "thumbnailImage": "/images/product.jpg",
      "stock": 50
    }
  ]
}
```

**Example:**
```bash
GET /shop/search?q=laptop&pageNumber=1
```

---

### Browse by Category

**GET** `/shop/category/{slug}`

**Path Parameters:**
- `slug` (string, required) - Category URL slug

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 12) - Items per page

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Product Name",
      "slug": "product-name",
      "originalPrice": 1000000,
      "salePrice": 800000,
      "thumbnailImage": "/images/product.jpg"
    }
  ]
}
```

**Example:**
```bash
GET /shop/category/electronics?pageNumber=1
```

---

## CART API

### View Shopping Cart

**GET** `/cart`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "productId": 1,
        "productName": "Product Name",
        "productImage": "/images/product.jpg",
        "quantity": 2,
        "unitPrice": 800000,
        "totalPrice": 1600000,
        "selectedAttributes": {
          "size": "M",
          "color": "Red"
        }
      }
    ],
    "subTotal": 1600000,
    "shippingCost": 25000,
    "taxAmount": 0,
    "discountAmount": 0,
    "appliedCoupon": null,
    "totalAmount": 1625000
  }
}
```

---

### Add Product to Cart

**POST** `/cart/add-to-cart`

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 1,
  "attributes": "{\"size\":\"M\",\"color\":\"Red\"}"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đã thêm vào giỏ hàng",
  "cartCount": 5
}
```

**Status Codes:**
- `200` - Product added successfully
- `400` - Product not found or invalid quantity

---

### Remove Item from Cart

**POST** `/cart/remove-item`

**Request Body:**
```json
{
  "productId": 1
}
```

**Response:**
```json
{
  "success": true,
  "message": "Đã xóa khỏi giỏ hàng"
}
```

---

### Update Item Quantity

**POST** `/cart/update-quantity`

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 3
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật thành công"
}
```

**Note:** If quantity is 0 or less, the item will be removed.

---

### Apply Discount Code

**POST** `/cart/apply-coupon`

**Request Body:**
```json
{
  "couponCode": "SAVE10"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Áp dụng mã giảm giá thành công",
  "discountAmount": 160000
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Mã giảm giá không hợp lệ"
}
```

**Status Codes:**
- `200` - Coupon applied
- `400` - Invalid coupon or minimum order not met

---

### Clear Shopping Cart

**POST** `/cart/clear`

**Response:**
```json
{
  "success": true,
  "message": "Giỏ hàng đã được làm trống"
}
```

---

## ACCOUNT API

### User Login

**GET** `/account/login` - Display login page

**POST** `/account/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Login successful",
  "redirectUrl": "/account/dashboard"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Email hoặc mật khẩu không chính xác"
}
```

---

### User Registration

**GET** `/account/register` - Display registration page

**POST** `/account/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "redirectUrl": "/account/login"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Email đã đăng ký",
  "errors": ["Email already registered"]
}
```

---

### Get Customer Dashboard

**GET** `/account/dashboard`

**Requires Authentication**

**Response:**
```json
{
  "success": true,
  "data": {
    "customerId": 1,
    "name": "John Doe",
    "email": "user@example.com",
    "loayaltyPoints": 500
  }
}
```

---

### View Order History

**GET** `/account/orders`

**Requires Authentication**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderNumber": "ORD-2024-001",
      "totalAmount": 1625000,
      "orderStatus": "delivered",
      "paymentStatus": "paid",
      "createdAt": "2024-03-01T10:00:00Z",
      "updatedAt": "2024-03-03T14:30:00Z",
      "trackingNumber": "VN123456789"
    }
  ]
}
```

---

### Get Order Details

**GET** `/account/order-detail/{id}`

**Path Parameters:**
- `id` (int, required) - Order ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderNumber": "ORD-2024-001",
    "items": [
      {
        "productId": 1,
        "productName": "Product Name",
        "productImage": "/images/product.jpg",
        "unitPrice": 800000,
        "quantity": 2,
        "totalPrice": 1600000
      }
    ],
    "totalAmount": 1625000,
    "orderStatus": "delivered",
    "paymentStatus": "paid",
    "paymentMethod": "bank_transfer",
    "createdAt": "2024-03-01T10:00:00Z",
    "trackingNumber": "VN123456789"
  }
}
```

---

### View Wishlist

**GET** `/account/wishlist`

**Requires Authentication**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Product Name",
      "slug": "product-name",
      "originalPrice": 1000000,
      "salePrice": 800000,
      "thumbnailImage": "/images/product.jpg",
      "averageRating": 4.5,
      "reviewCount": 10,
      "stock": 50
    }
  ]
}
```

---

### User Logout

**GET** `/account/logout`

**Response:**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

## ADMIN API

### Admin Dashboard

**GET** `/admin/dashboard`

**Requires Admin Authentication**

**Response:**
```json
{
  "success": true,
  "data": {
    "totalProducts": 150,
    "totalCustomers": 500,
    "totalOrders": 1200,
    "totalRevenue": 5000000,
    "recentOrders": [...]
  }
}
```

---

### List Products (Admin)

**GET** `/admin/products`

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20)

**Requires Admin Authentication**

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Product Name",
      "slug": "product-name",
      "sku": "SKU-001",
      "price": 800000,
      "stock": 50,
      "categoryId": 1,
      "isActive": true,
      "createdAt": "2024-03-01T10:00:00Z"
    }
  ],
  "pagination": {
    "totalPages": 8,
    "currentPage": 1
  }
}
```

---

### Create Product

**POST** `/admin/products/create`

**Request Body:**
```json
{
  "name": "New Product",
  "description": "Product description",
  "detailDescription": "Detailed description",
  "originalPrice": 1000000,
  "salePrice": 800000,
  "stock": 100,
  "brand": "Brand Name",
  "categoryId": 1,
  "thumbnailImage": "/images/thumb.jpg",
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": 1,
    "name": "New Product",
    "slug": "new-product"
  }
}
```

---

### Edit Product

**POST** `/admin/products/edit/{id}`

**Path Parameters:**
- `id` (int, required) - Product ID

**Request Body:**
```json
{
  "id": 1,
  "name": "Updated Product",
  "description": "Updated description",
  "originalPrice": 900000,
  "salePrice": 750000,
  "stock": 75
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product updated successfully"
}
```

---

### Delete Product

**POST** `/admin/products/delete/{id}`

**Path Parameters:**
- `id` (int, required) - Product ID

**Response:**
```json
{
  "success": true,
  "message": "Product deleted successfully"
}
```

---

### List Orders (Admin)

**GET** `/admin/orders`

**Query Parameters:**
- `status` (string, optional) - Filter by status
  - `pending`
  - `confirmed`
  - `processing`
  - `shipping`
  - `delivered`
  - `cancelled`
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderNumber": "ORD-2024-001",
      "customerName": "John Doe",
      "totalAmount": 1625000,
      "orderStatus": "processing",
      "paymentStatus": "paid",
      "createdAt": "2024-03-01T10:00:00Z"
    }
  ]
}
```

---

### Get Order Details (Admin)

**GET** `/admin/orders/detail/{id}`

**Path Parameters:**
- `id` (int, required) - Order ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderNumber": "ORD-2024-001",
    "customer": {
      "id": 1,
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    },
    "items": [...],
    "orderStatus": "processing",
    "paymentStatus": "paid"
  }
}
```

---

### Update Order Status

**POST** `/admin/orders/update-status`

**Request Body:**
```json
{
  "orderId": 1,
  "status": "shipped"
}
```

**Allowed Statuses:**
- `pending`
- `confirmed`
- `processing`
- `shipping`
- `delivered`
- `cancelled`

**Response:**
```json
{
  "success": true,
  "message": "Order status updated successfully"
}
```

---

### List Discount Codes

**GET** `/admin/discounts`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "code": "SAVE10",
      "description": "10% discount on all products",
      "discountType": "percentage",
      "discountValue": 10,
      "usageLimit": 100,
      "timesUsed": 25,
      "isActive": true,
      "startDate": "2024-03-01T00:00:00Z",
      "endDate": "2024-03-31T23:59:59Z"
    }
  ]
}
```

---

### Create Discount Code

**POST** `/admin/discounts/create`

**Request Body:**
```json
{
  "code": "SAVE10",
  "description": "10% discount on all products",
  "discountType": "percentage",
  "discountValue": 10,
  "usageLimit": 100,
  "minOrderAmount": 100000,
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z",
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Discount code created successfully",
  "data": {
    "id": 1,
    "code": "SAVE10"
  }
}
```

---

### Delete Discount Code

**POST** `/admin/discounts/delete/{id}`

**Path Parameters:**
- `id` (int, required) - Discount ID

**Response:**
```json
{
  "success": true,
  "message": "Discount code deleted successfully"
}
```

---

## Error Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request succeeded |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Resource already exists |
| 500 | Internal Server Error |

---

## Rate Limiting

Currently, there is no rate limiting. Future versions will implement:
- 100 requests per minute per IP
- 1000 requests per minute per authenticated user

---

## Versioning

Current API version: v1

---

## Support

For API support, contact: support@ecommerceapp.com
