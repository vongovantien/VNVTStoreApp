# Reference Attributes Documentation

Hướng dẫn sử dụng `ReferenceAttribute` và `ReferenceCollectionAttribute` trong Dapper queries.

---

## Overview

| Attribute | Mục đích | SQL Generated |
|-----------|----------|---------------|
| `[Reference]` | Map **1 column** từ bảng khác | `LEFT JOIN` |
| `[ReferenceCollection]` | Map **collection** từ bảng con | Second Query |

---

## 1. ReferenceAttribute

### Cú pháp cơ bản

```csharp
[Reference(tableName, foreignKey, selectColumn)]
```

| Parameter | Mô tả | Ví dụ |
|-----------|-------|-------|
| `tableName` | Tên bảng tham chiếu | `"TblCategory"` |
| `foreignKey` | Cột FK trên bảng hiện tại | `"CategoryCode"` |
| `selectColumn` | Cột muốn lấy từ bảng tham chiếu | `"Name"` |

### Ví dụ 1: Lookup đơn giản

```csharp
public class ProductDto
{
    public string CategoryCode { get; set; }
    
    [Reference("TblCategory", "CategoryCode", "Name")]
    public string? CategoryName { get; set; }
}
```

**SQL Generated:**
```sql
SELECT r.*, t1."Name" AS "CategoryName"
FROM "TblProduct" AS r
LEFT JOIN "TblCategory" AS t1 ON r."CategoryCode" = t1."Code"
```

### Ví dụ 2: Lookup User

```csharp
public class OrderDto
{
    public string UserCode { get; set; }
    
    [Reference("TblUser", "UserCode", "FullName")]
    public string? CustomerName { get; set; }
}
```

---

## 2. ReferenceAttribute với Polymorphic Join

### Cú pháp đầy đủ

```csharp
[Reference(tableName, foreignKey, selectColumn, 
           TargetColumn = "...", 
           FilterColumn = "...", 
           FilterValue = "...")]
```

| Parameter | Mô tả | Mặc định |
|-----------|-------|----------|
| `TargetColumn` | Cột tham chiếu trên bảng đích (thay vì "Code") | `"Code"` |
| `FilterColumn` | Cột dùng để filter | `null` |
| `FilterValue` | Giá trị filter | `null` |

### Ví dụ: Lấy ảnh sản phẩm từ TblFile

```csharp
public class OrderItemDto
{
    public string? ProductCode { get; set; }
    
    [Reference("TblFile", "ProductCode", "Path", 
               TargetColumn = "MasterCode", 
               FilterColumn = "MasterType", 
               FilterValue = "Product")]
    public string? ProductImage { get; set; }
}
```

**SQL Generated:**
```sql
LEFT JOIN "TblFile" AS t1 
    ON r."ProductCode" = t1."MasterCode" 
   AND t1."MasterType" = 'Product'
```

---

## 3. ReferenceCollectionAttribute

### Cú pháp

```csharp
[ReferenceCollection(childDtoType, childTableName, foreignKey, parentKey, filterColumn?, filterValue?)]
```

| Parameter | Mô tả |
|-----------|-------|
| `childDtoType` | `typeof(ChildDto)` |
| `childTableName` | Tên bảng con |
| `foreignKey` | FK trên bảng con |
| `parentKey` | PK trên bảng cha (DTO property) |
| `filterColumn` | (Optional) Cột filter |
| `filterValue` | (Optional) Giá trị filter |

### Ví dụ 1: Order Items

```csharp
public class OrderDto
{
    public string Code { get; set; }
    
    [ReferenceCollection(typeof(OrderItemDto), "TblOrderItem", "OrderCode", "Code")]
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
```

**Second Query Generated:**
```sql
SELECT * FROM "TblOrderItem" WHERE "OrderCode" = ANY(@Codes)
```

### Ví dụ 2: Product Images với Filter

```csharp
public class ProductDto
{
    public string Code { get; set; }
    
    [ReferenceCollection(typeof(ProductImageDto), "TblFile", "MasterCode", "Code", "MasterType", "Product")]
    public List<ProductImageDto> ProductImages { get; set; } = new();
}
```

**Second Query Generated:**
```sql
SELECT * FROM "TblFile" 
WHERE "MasterCode" = ANY(@Codes) 
  AND "MasterType" = @FilterValue
```

---

## Quick Reference

### So sánh

| Feature | ReferenceAttribute | ReferenceCollectionAttribute |
|---------|-------------------|------------------------------|
| Returns | 1 value | List of values |
| SQL | LEFT JOIN | Separate Query |
| Cardinality | N:1 | 1:N |
| Performance | Fast | Additional round-trip |

### Khi nào dùng gì?

| Use Case | Attribute |
|----------|-----------|
| Lấy tên Category từ CategoryCode | `[Reference]` |
| Lấy tên User từ UserCode | `[Reference]` |
| Lấy danh sách OrderItems | `[ReferenceCollection]` |
| Lấy danh sách Images | `[ReferenceCollection]` |
| Lấy 1 ảnh đại diện | `[Reference]` với filter |

---

## Lưu ý quan trọng

- `ReferenceAttribute` mặc định join với cột `Code` của bảng đích
- Dùng `TargetColumn` để join với cột khác (ví dụ: `MasterCode`)
- Properties được đánh `[Reference]` sẽ **tự động được fill** bởi `BaseHandler.GetPagedDapperAsync()`

**Tip:** Để tối ưu performance, chỉ dùng `[ReferenceCollection]` khi thực sự cần. Mỗi collection = 1 query thêm.
