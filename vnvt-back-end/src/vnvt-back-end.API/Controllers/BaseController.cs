using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TDto> : ControllerBase
       where TDto : class, IBaseDto
    {
        private readonly IBaseService<TDto> _baseService;

        protected BaseController(IBaseService<TDto> baseService)
        {
            _baseService = baseService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TDto>>>> GetAll()
        {
            var response = await _baseService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("paging")]
        public async Task<ActionResult<ApiResponse<PagedResult<TDto>>>> GetPaging([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _baseService.GetPagingAsync(pagingParameters);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TDto>>> GetById(int id)
        {
            var response = await _baseService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TDto>>> Create(TDto dto)
        {
            var response = await _baseService.AddAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TDto>>> Update(int id, TDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponseBuilder.Error<TDto>("Invalid ID", 400));
            }

            var response = await _baseService.UpdateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var response = await _baseService.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
