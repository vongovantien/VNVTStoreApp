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
    public long Id { get; set; }

    [Column("image_url")]
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [Column("product_id")]
    public long? ProductId { get; set; }
}
