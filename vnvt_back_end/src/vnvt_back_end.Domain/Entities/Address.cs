using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace vnvt_back_end.Infrastructure;

[Table("addresses")]
[Index("City", Name = "idx_addresses_city")]
[Index("PostalCode", Name = "idx_addresses_postal_code")]
[Index("UserId", Name = "idx_addresses_user_id")]
public partial class Address
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("street_address")]
    [StringLength(255)]
    public string StreetAddress { get; set; } = null!;

    [Column("city")]
    [StringLength(100)]
    public string City { get; set; } = null!;

    [Column("state")]
    [StringLength(100)]
    public string State { get; set; } = null!;

    [Column("postal_code")]
    [StringLength(20)]
    public string PostalCode { get; set; } = null!;

    [Column("country")]
    [StringLength(100)]
    public string Country { get; set; } = null!;

    [Column("createddate", TypeName = "timestamp without time zone")]
    public DateTime? Createddate { get; set; }

    [Column("updateddate", TypeName = "timestamp without time zone")]
    public DateTime? Updateddate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Addresses")]
    public virtual User? User { get; set; }
}
