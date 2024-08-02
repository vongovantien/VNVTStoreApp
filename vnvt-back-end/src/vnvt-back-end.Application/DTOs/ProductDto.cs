using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.DTOs
{
    public class ProductDto : IBaseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? Createddate { get; set; }
        public DateTime? Updateddate { get; set; }
    }
}
