using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities
{
    public class TblSystemConfig : BaseEntity
    {
        public string? ConfigValue { get; set; } // JSON string or simple value

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
