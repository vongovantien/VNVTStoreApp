using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("product_images")]
public partial class ProductImage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string ImageUrl { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductImages")]
    public virtual Product? Product { get; set; }
}
