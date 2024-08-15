using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("orders")]
[Index("OrderStatus", Name = "idx_orders_order_status")]
[Index("UserId", Name = "idx_orders_user_id")]
public partial class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("order_status")]
    [StringLength(50)]
    public string OrderStatus { get; set; } = null!;

    [Column("total_amount")]
    [Precision(10, 2)]
    public decimal TotalAmount { get; set; }

    [Column("createddate", TypeName = "timestamp without time zone")]
    public DateTime? Createddate { get; set; }

    [Column("updateddate", TypeName = "timestamp without time zone")]
    public DateTime? Updateddate { get; set; }

    [Column("coupon_id")]
    public int? CouponId { get; set; }

    [Column("address")]
    [StringLength(100)]
    public string? Address { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("last_name")]
    [StringLength(100)]
    public string? LastName { get; set; }

    [Column("note")]
    [StringLength(100)]
    public string? Note { get; set; }

    [Column("order_date", TypeName = "timestamp without time zone")]
    public DateTime? OrderDate { get; set; }

    [Column("payment_method")]
    [StringLength(255)]
    public string? PaymentMethod { get; set; }

    [Column("shipping_address")]
    [StringLength(255)]
    public string? ShippingAddress { get; set; }

    [Column("shipping_date", TypeName = "timestamp without time zone")]
    public DateTime? ShippingDate { get; set; }

    [Column("shipping_method")]
    [StringLength(255)]
    public string? ShippingMethod { get; set; }

    [Column("status")]
    [StringLength(255)]
    public string? Status { get; set; }

    [Column("first_name", TypeName = "character varying")]
    public string? FirstName { get; set; }

    [Column("city", TypeName = "character varying")]
    public string? City { get; set; }

    [Column("country", TypeName = "character varying")]
    public string? Country { get; set; }

    [Column("zipcode", TypeName = "character varying")]
    public string? Zipcode { get; set; }

    [ForeignKey("CouponId")]
    [InverseProperty("Orders")]
    public virtual Coupon? Coupon { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }
}
