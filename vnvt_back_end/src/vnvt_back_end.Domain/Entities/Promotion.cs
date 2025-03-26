using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("promotions")]
[Index("PromotionCode", Name = "promotions_promotion_code_key", IsUnique = true)]
public partial class Promotion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("promotion_code")]
    [StringLength(50)]
    public string PromotionCode { get; set; } = null!;

    [Column("discount_percentage")]
    [Precision(5, 2)]
    public decimal DiscountPercentage { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("description")]
    [StringLength(10000)]
    public string? Description { get; set; }

    [InverseProperty("Promotion")]
    public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
}
