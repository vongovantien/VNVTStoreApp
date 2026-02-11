using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities
{
    public class TblSystemConfig : IEntity
    {
        [Key]
        [MaxLength(50)]
        public string Code { get; set; } = null!; // e.g., "FLASHSALE_SCHEDULE"

        public string? ConfigValue { get; set; } // JSON string or simple value

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? ModifiedType { get; set; }
    }
}
