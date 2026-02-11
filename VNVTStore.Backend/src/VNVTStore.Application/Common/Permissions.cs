namespace VNVTStore.Application.Common;

public static class Permissions
{
    // Products Module
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Update = "Permissions.Products.Update";
        public const string Delete = "Permissions.Products.Delete";
        public const string Import = "Permissions.Products.Import";
        public const string Export = "Permissions.Products.Export";
    }

    // Categories Module
    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Update = "Permissions.Categories.Update";
        public const string Delete = "Permissions.Categories.Delete";
    }

    // Brands Module
    public static class Brands
    {
        public const string View = "Permissions.Brands.View";
        public const string Create = "Permissions.Brands.Create";
        public const string Update = "Permissions.Brands.Update";
        public const string Delete = "Permissions.Brands.Delete";
    }

    // Orders Module
    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string Create = "Permissions.Orders.Create";
        public const string Update = "Permissions.Orders.Update";
        public const string Cancel = "Permissions.Orders.Cancel";
        public const string Export = "Permissions.Orders.Export";
    }

    // Customers Module
    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Create = "Permissions.Customers.Create";
        public const string Update = "Permissions.Customers.Update";
        public const string Delete = "Permissions.Customers.Delete";
    }

    // Inventory/Suppliers Module
    public static class Inventory
    {
        public const string View = "Permissions.Inventory.View";
        public const string Manage = "Permissions.Inventory.Manage";
    }

    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Update = "Permissions.Suppliers.Update";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    // Banners Module
    public static class Banners
    {
        public const string View = "Permissions.Banners.View";
        public const string Create = "Permissions.Banners.Create";
        public const string Update = "Permissions.Banners.Update";
        public const string Delete = "Permissions.Banners.Delete";
    }

    // News Module
    public static class News
    {
        public const string View = "Permissions.News.View";
        public const string Create = "Permissions.News.Create";
        public const string Update = "Permissions.News.Update";
        public const string Delete = "Permissions.News.Delete";
    }

    // Coupons Module
    public static class Coupons
    {
        public const string View = "Permissions.Coupons.View";
        public const string Create = "Permissions.Coupons.Create";
        public const string Update = "Permissions.Coupons.Update";
        public const string Delete = "Permissions.Coupons.Delete";
    }

    // Promotions Module
    public static class Promotions
    {
        public const string View = "Permissions.Promotions.View";
        public const string Create = "Permissions.Promotions.Create";
        public const string Update = "Permissions.Promotions.Update";
        public const string Delete = "Permissions.Promotions.Delete";
    }

    // Reviews Module
    public static class Reviews
    {
        public const string View = "Permissions.Reviews.View";
        public const string Moderate = "Permissions.Reviews.Moderate";
        public const string Delete = "Permissions.Reviews.Delete";
    }

    // Reports/Dashboard Module
    public static class Reports
    {
        public const string ViewDashboard = "Permissions.Reports.ViewDashboard";
        public const string ViewSales = "Permissions.Reports.ViewSales";
        public const string ViewInventory = "Permissions.Reports.ViewInventory";
        public const string ViewProfit = "Permissions.Reports.ViewProfit";
        public const string Export = "Permissions.Reports.Export";
    }

    // Settings Module
    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string ManageRoles = "Permissions.Settings.ManageRoles";
        public const string ManageUsers = "Permissions.Settings.ManageUsers";
        public const string ManageSystem = "Permissions.Settings.ManageSystem";
    }

    // Units Module
    public static class Units
    {
        public const string View = "Permissions.Units.View";
        public const string Create = "Permissions.Units.Create";
        public const string Update = "Permissions.Units.Update";
        public const string Delete = "Permissions.Units.Delete";
    }

    // Tags Module
    public static class Tags
    {
        public const string View = "Permissions.Tags.View";
        public const string Create = "Permissions.Tags.Create";
        public const string Update = "Permissions.Tags.Update";
        public const string Delete = "Permissions.Tags.Delete";
    }

    // Quotes Module
    public static class Quotes
    {
        public const string View = "Permissions.Quotes.View";
        public const string Create = "Permissions.Quotes.Create";
        public const string Update = "Permissions.Quotes.Update";
        public const string Delete = "Permissions.Quotes.Delete";
    }

    // Channel Access (for middleware)
    public static class Channel
    {
        public const string AccessWeb = "Permissions.Channel.Access.Web";
        public const string AccessPos = "Permissions.Channel.Access.Pos";
        public const string AccessApp = "Permissions.Channel.Access.App";
    }
}
