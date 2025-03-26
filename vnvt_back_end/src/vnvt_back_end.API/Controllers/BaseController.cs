using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.Caching;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;

namespace vnvt_back_end.API.Controllers
{
    [ApiVersion("1.0")]
    //[MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class BaseController<TDto, TEntity> : ControllerBase
        where TEntity : class
       where TDto : class, IBaseDto
    {
        private readonly IBaseService<TDto, TEntity> _baseService;

        protected BaseController(IBaseService<TDto, TEntity> baseService)
        {
            _baseService = baseService;
        }

        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<IEnumerable<TDto>>>> GetAll()
        {
            var response = await _baseService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("paging")]
        public virtual async Task<ActionResult<ApiResponse<PagedResult<TDto>>>> GetPaging([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _baseService.GetPagedAsync(pagingParameters);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<TDto>>> GetById(int id)
        {
            var response = await _baseService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TDto>>> Create(TDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(ApiResponseBuilder.BadRequest<TDto>("Invalid order data."));
            }
            var response = await _baseService.AddAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TDto>>> Update(int id, TDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponseBuilder.BadRequest<TDto>("Invalid ID"));
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

        [HttpPost]
        public virtual async Task<IActionResult> Search([FromBody] RequestDTO request)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                if (request.Searching == null && request.SortDTO == null)
                {
                    response.Code = _resultCode.NullParams.Code;
                    response.Message = _resultCode.NullParams.Message;
                }
                else
                {
                    List<SearchDTO> searching = request.Searching;
                    string companyCode = null;
                    if (request.PostObject != null)
                    {
                        companyCode = ((request.PostObject is string) ? (request.PostObject as string) : null);
                    }

                    SortDTO sortDTO = request.SortDTO;
                    if (searching != null && searching.Count > 0)
                    {
                        if (request.PageIndex.HasValue && request.PageSize.HasValue)
                        {
                            if (request.Fields == null)
                            {
                                ResultDTO<TEntity> obj = await _baseService.SearchPaging(searching, request.PageSize.Value, request.PageIndex.Value, sortDTO, companyCode, _dataLevel);
                                long total = obj.Total;
                                IEnumerable<TEntity> data = obj.Data;
                                response.Data = data;
                                response.Total = total;
                            }
                            else
                            {
                                ResultDTO<object> obj2 = await _baseService.SearchPagingFieldsAsync(searching, request.PageSize.Value, request.PageIndex.Value, request.Fields, sortDTO, companyCode, _dataLevel);
                                long total2 = obj2.Total;
                                IEnumerable<object> data2 = obj2.Data;
                                response.Data = data2;
                                response.Total = total2;
                            }
                        }
                        else
                        {
                            object obj3 = ((request.Fields != null) ? (await _baseService.SearchFieldsAysnc(searching, request.Fields, sortDTO, companyCode, _dataLevel)) : (await _baseService.Search(searching, sortDTO, companyCode, _dataLevel)));
                            dynamic val = obj3;
                            dynamic val2 = val.Count;
                            response.Data = (object)val;
                            response.Total = val2;
                        }

                        response.Code = _resultCode.Ok.Code;
                        response.Message = _resultCode.Ok.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FriendlyException)
                {
                    response.Code = _resultCode.Friendly.Code;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Code = _resultCode.HandleRequestFail.Code;
                    response.Message = _resultCode.HandleRequestFail.Message;
                }
            }

            return Ok(response);
        }
    }
}
