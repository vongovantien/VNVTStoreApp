using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities
{
    [Table("Banner")]
    public class TblBanner : IEntity
    {
        [Key]
        [MaxLength(10)]
        public string Code { get; set; } = null!; // e.g. BNN000001
        
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Content { get; set; }
        
        [MaxLength(200)]
        public string? LinkUrl { get; set; }
        
        [MaxLength(150)] // Increased from 50
        public string? LinkText { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int Priority { get; set; } = 0;
        
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public string? ModifiedType { get; set; }

        public void Update(string title, string? content, string? linkUrl, string? linkText, bool isActive, int priority)
        {
            Title = title;
            Content = content;
            LinkUrl = linkUrl;
            LinkText = linkText;
            IsActive = isActive;
            Priority = priority;
            UpdatedAt = DateTime.Now;
        }
    }
}
