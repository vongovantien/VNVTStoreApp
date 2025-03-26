using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static vnvt_back_end.Application.DTOs.DTOs;
using vnvt_back_end.Infrastructure;
using vnvt_back_end.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Services;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : BaseController<CategoryDto, Category>
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
           : base(categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        public override async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
        {
            var response = await base.GetAll();
            return response;
        }
    }
}
