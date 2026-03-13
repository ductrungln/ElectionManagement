using System;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceApp.Helpers
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash);
        }
    }

    public class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            text = text.Trim().ToLower();
            // Remove Vietnamese diacritics
            text = RemoveDiacritics(text);
            // Replace spaces with dashes
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "-");
            // Remove any non-alphanumeric characters except dashes
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\-]", "");
            // Remove multiple consecutive dashes
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\-+", "-");
            // Remove leading/trailing dashes
            text = text.Trim('-');

            return text;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }

    public class FileHelper
    {
        public static string GenerateFileName(string originalFileName)
        {
            var extension = System.IO.Path.GetExtension(originalFileName);
            var fileName = $"{Guid.NewGuid().ToString()}{extension}";
            return fileName;
        }

        public static bool IsValidImageFile(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName).ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return Array.Exists(validExtensions, ext => ext.Equals(extension));
        }
    }

    public class PaginationHelper
    {
        public static int CalculateTotalPages(int totalItems, int pageSize)
        {
            return (int)System.Math.Ceiling((decimal)totalItems / pageSize);
        }

        public static int CalculateSkip(int pageNumber, int pageSize)
        {
            return (pageNumber - 1) * pageSize;
        }
    }
}
