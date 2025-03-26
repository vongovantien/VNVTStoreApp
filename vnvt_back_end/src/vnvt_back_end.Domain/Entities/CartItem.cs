using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("cart_items")]
public partial class CartItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("cart_id")]
    public int? CartId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("CartId")]
    [InverseProperty("CartItems")]
    public virtual Cart? Cart { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("CartItems")]
    public virtual Product? Product { get; set; }
}
