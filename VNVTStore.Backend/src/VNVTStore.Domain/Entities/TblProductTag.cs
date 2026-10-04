using System;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public class TblProductTag : BaseEntity
{
    public string ProductCode { get; set; } = null!;
    public string TagCode { get; set; } = null!;

    public virtual TblProduct Product { get; set; } = null!;
    public virtual TblTag Tag { get; set; } = null!;
}
