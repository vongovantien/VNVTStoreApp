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

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }
}
