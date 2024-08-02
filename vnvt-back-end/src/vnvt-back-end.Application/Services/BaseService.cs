using AutoMapper;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;

namespace vnvt_back_end.Application.Services
{
    public abstract class BaseService<TEntity, TDto> : IBaseService<TDto>
        where TEntity : class
        where TDto : class, IBaseDto
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;

        protected BaseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<TDto>>> GetAllAsync()
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var result = await repository.GetAllAsync();
            var items = _mapper.Map<IEnumerable<TDto>>(result);
            return ApiResponseBuilder.Success(items);
        }

        public async Task<ApiResponse<PagedResult<TDto>>> GetPagingAsync(PagingParameters pagingParameters)
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var pagedResult = await repository.GetPagedAsync(pagingParameters);
            var items = _mapper.Map<IEnumerable<TDto>>(pagedResult.Items);
            var result = new PagedResult<TDto>(items, pagedResult.TotalItems, pagedResult.PageNumber, pagedResult.PageSize);
            return ApiResponseBuilder.Success(result);
        }

        public async Task<ApiResponse<TDto>> GetByIdAsync(int id)
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
            {
                return ApiResponseBuilder.NotFound<TDto>();
            }

            var dto = _mapper.Map<TDto>(entity);
            return ApiResponseBuilder.Success(dto);
        }

        public async Task<ApiResponse<TDto>> AddAsync(TDto dto)
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var entity = _mapper.Map<TEntity>(dto);
            await repository.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<TDto>(entity);
            return ApiResponseBuilder.Created(resultDto);
        }

        public async Task<ApiResponse<TDto>> UpdateAsync(TDto dto)
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var entity = await repository.GetByIdAsync(dto.Id);
            if (entity == null)
            {
                return ApiResponseBuilder.NotFound<TDto>();
            }

            _mapper.Map(dto, entity);
            repository.Update(entity);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<TDto>(entity);
            return ApiResponseBuilder.Success(resultDto);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var repository = _unitOfWork.GetRepository<TEntity>();
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
            {
                return ApiResponseBuilder.NotFound<bool>();
            }

            repository.Delete(entity);
            await _unitOfWork.CommitAsync();

            return ApiResponseBuilder.Success(true, "Entity deleted successfully");
        }
    }
}
