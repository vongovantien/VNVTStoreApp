using AutoMapper;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class ReviewService : BaseService<Review, ReviewDto>, IReviewService
    {
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
    }
}
