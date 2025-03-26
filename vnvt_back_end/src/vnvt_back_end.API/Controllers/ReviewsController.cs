using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    //[MiddlewareFilter(typeof(RequestLocalizationMiddleware))]
    [ApiController]
    public class ReviewsController : BaseController<ReviewDto, Review>
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
           : base(reviewService)
        {
            _reviewService = reviewService;
        }
    }
}
