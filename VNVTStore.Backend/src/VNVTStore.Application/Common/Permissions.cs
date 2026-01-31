namespace VNVTStore.Application.Common;

public static class Permissions
{
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Update = "Permissions.Products.Update";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string Manage = "Permissions.Orders.Manage";
        public const string Cancel = "Permissions.Orders.Cancel";
    }

    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Manage = "Permissions.Customers.Manage";
    }

    public static class Inventory
    {
        public const string View = "Permissions.Inventory.View";
        public const string Manage = "Permissions.Inventory.Manage";
    }

    public static class Reports
    {
        public const string ViewSales = "Permissions.Reports.ViewSales";
        public const string ViewInventory = "Permissions.Reports.ViewInventory";
        public const string ViewProfit = "Permissions.Reports.ViewProfit";
    }

    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string ManageRoles = "Permissions.Settings.ManageRoles";
        public const string ManageUsers = "Permissions.Settings.ManageUsers";
    }
}
