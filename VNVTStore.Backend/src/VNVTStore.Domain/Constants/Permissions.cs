namespace VNVTStore.Domain.Constants;

/// <summary>
/// Centralized permission constants to avoid magic strings.
/// Usage: [HasPermission(Permissions.Products.Delete)]
/// </summary>
public static class Permissions
{
    public static class Products
    {
        public const string View = "products:view";
        public const string Create = "products:create";
        public const string Edit = "products:edit";
        public const string Delete = "products:delete";
        public const string Import = "products:import";
        public const string Export = "products:export";
    }

    public static class Orders
    {
        public const string View = "orders:view";
        public const string Create = "orders:create";
        public const string Process = "orders:process";
        public const string Cancel = "orders:cancel";
        public const string Export = "orders:export";
    }

    public static class Customers
    {
        public const string View = "customers:view";
        public const string Create = "customers:create";
        public const string Edit = "customers:edit";
        public const string Delete = "customers:delete";
    }

    public static class Categories
    {
        public const string View = "categories:view";
        public const string Create = "categories:create";
        public const string Edit = "categories:edit";
        public const string Delete = "categories:delete";
    }

    public static class Roles
    {
        public const string View = "roles:view";
        public const string Create = "roles:create";
        public const string Edit = "roles:edit";
        public const string Delete = "roles:delete";
        public const string ManagePermissions = "roles:manage-permissions";
    }

    public static class Users
    {
        public const string View = "users:view";
        public const string Create = "users:create";
        public const string Edit = "users:edit";
        public const string Delete = "users:delete";
        public const string ResetPassword = "users:reset-password";
    }

    public static class Reports
    {
        public const string View = "reports:view";
        public const string Export = "reports:export";
    }

    public static class Settings
    {
        public const string View = "settings:view";
        public const string Edit = "settings:edit";
    }
}
