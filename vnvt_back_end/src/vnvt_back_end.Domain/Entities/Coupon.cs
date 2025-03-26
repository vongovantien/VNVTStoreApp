using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("coupons")]
[Index("CouponCode", Name = "coupons_coupon_code_key", IsUnique = true)]
public partial class Coupon
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("coupon_code")]
    [StringLength(50)]
    public string CouponCode { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("discount_percentage")]
    [Precision(5, 2)]
    public decimal DiscountPercentage { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("max_uses")]
    public int MaxUses { get; set; }

    [Column("createddate", TypeName = "timestamp without time zone")]
    public DateTime? Createddate { get; set; }

    [Column("updateddate", TypeName = "timestamp without time zone")]
    public DateTime? Updateddate { get; set; }

    [InverseProperty("Coupon")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
