namespace VNVTStore.Application.Constants;

/// <summary>
/// Các hằng số ứng dụng
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Paging Constants
    /// </summary>
    public static class Paging
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 50;
        public const int MinPageSize = 1;
    }

    /// <summary>
    /// Validation Constants
    /// </summary>
    public static class Validation
    {
        // User
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 50;
        public const int EmailMaxLength = 100;
        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 255;
        public const int NameMaxLength = 100;

        // Product
        public const int ProductNameMaxLength = 100;
        public const int ProductDescriptionMaxLength = 10000;
        public const decimal ProductMinPrice = 0.01m;
        public const int ProductMinStock = 0;

        // Category
        public const int CategoryNameMaxLength = 100;

        // Order
        public const int OrderNoteMaxLength = 500;
        public const int AddressMaxLength = 255;

        // Review
        public const int ReviewMinRating = 1;
        public const int ReviewMaxRating = 5;
        public const int ReviewCommentMaxLength = 1000;

        // General
        public const int UrlMaxLength = 500;
        public const int CodeMaxLength = 50;
    }

    /// <summary>
    /// HTTP Status Codes
    /// </summary>
    public static class StatusCodes
    {
        public const int Ok = 200;
        public const int Created = 201;
        public const int NoContent = 204;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int ValidationError = 422;
        public const int InternalServerError = 500;
    }

    /// <summary>
    /// Error Messages
    /// </summary>
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found.";
        public const string UserAlreadyExists = "User already exists.";
        public const string InvalidCredentials = "Invalid username or password.";
        public const string ProductNotFound = "Product not found.";
        public const string CategoryNotFound = "Category not found.";
        public const string OrderNotFound = "Order not found.";
        public const string InsufficientStock = "Insufficient stock.";
        public const string InvalidInput = "Invalid input.";
        public const string Unauthorized = "Unauthorized access.";
        public const string ValidationFailed = "Validation failed.";
    }

    /// <summary>
    /// Success Messages
    /// </summary>
    public static class SuccessMessages
    {
        public const string OperationSucceeded = "Operation succeeded.";
        public const string ResourceCreated = "Resource created successfully.";
        public const string ResourceUpdated = "Resource updated successfully.";
        public const string ResourceDeleted = "Resource deleted successfully.";
        public const string LoginSuccessful = "Login successful.";
        public const string RegisterSuccessful = "User registered successfully.";
    }
}
