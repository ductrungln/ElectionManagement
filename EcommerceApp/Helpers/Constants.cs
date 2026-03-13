using System;
using System.Collections.Generic;

namespace EcommerceApp.Helpers
{
    public class Constants
    {
        // Order Statuses
        public const string ORDER_STATUS_PENDING = "pending";
        public const string ORDER_STATUS_CONFIRMED = "confirmed";
        public const string ORDER_STATUS_PROCESSING = "processing";
        public const string ORDER_STATUS_SHIPPING = "shipping";
        public const string ORDER_STATUS_DELIVERED = "delivered";
        public const string ORDER_STATUS_CANCELLED = "cancelled";

        // Payment Statuses
        public const string PAYMENT_STATUS_UNPAID = "unpaid";
        public const string PAYMENT_STATUS_PAID = "paid";
        public const string PAYMENT_STATUS_REFUNDED = "refunded";
        public const string PAYMENT_STATUS_PARTIAL = "partial";

        // Payment Methods
        public const string PAYMENT_METHOD_COD = "cod";
        public const string PAYMENT_METHOD_BANK = "bank_transfer";
        public const string PAYMENT_METHOD_WALLET = "wallet";
        public const string PAYMENT_METHOD_ONLINE = "online_gateway";

        // Discount Types
        public const string DISCOUNT_TYPE_PERCENTAGE = "percentage";
        public const string DISCOUNT_TYPE_FIXED = "fixed";

        // Customer Groups
        public const string CUSTOMER_GROUP_NEW = "new";
        public const string CUSTOMER_GROUP_REGULAR = "regular";
        public const string CUSTOMER_GROUP_VIP = "vip";

        // Address Types
        public const string ADDRESS_TYPE_HOME = "home";
        public const string ADDRESS_TYPE_OFFICE = "office";
        public const string ADDRESS_TYPE_OTHER = "other";

        // Pagination
        public const int DEFAULT_PAGE_SIZE = 12;
        public const int MAX_PAGE_SIZE = 100;

        // File Upload
        public const long MAX_FILE_SIZE = 5242880; // 5MB
        public const string UPLOAD_PATH = "uploads";
        public const string PRODUCT_IMAGES_PATH = "uploads/products";
    }
}
