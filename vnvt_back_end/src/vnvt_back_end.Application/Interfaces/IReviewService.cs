using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IReviewService : IBaseService<ReviewDto, Review>
    {
    }
}
