using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("product_promotions")]
public partial class ProductPromotion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("promotion_id")]
    public int? PromotionId { get; set; }

    [Column("createddate", TypeName = "timestamp without time zone")]
    public DateTime? Createddate { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductPromotions")]
    public virtual Product? Product { get; set; }

    [ForeignKey("PromotionId")]
    [InverseProperty("ProductPromotions")]
    public virtual Promotion? Promotion { get; set; }
}
