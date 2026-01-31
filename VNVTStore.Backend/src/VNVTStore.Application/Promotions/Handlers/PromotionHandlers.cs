using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Promotions.Handlers;

public class PromotionHandlers : 
    IRequestHandler<CreateCommand<CreatePromotionDto, PromotionDto>, Result<PromotionDto>>,
    IRequestHandler<UpdateCommand<UpdatePromotionDto, PromotionDto>, Result<PromotionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRepository<TblPromotion> _promotionRepository;

    public PromotionHandlers(IUnitOfWork unitOfWork, IMapper mapper, IRepository<TblPromotion> promotionRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<PromotionDto>> Handle(CreateCommand<CreatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var entity = _mapper.Map<TblPromotion>(dto);
        
        if (string.IsNullOrEmpty(entity.Code))
        {
             entity.Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }

        if (dto.ProductCodes != null && dto.ProductCodes.Any())
        {
            foreach (var productCode in dto.ProductCodes)
            {
                entity.TblProductPromotions.Add(new TblProductPromotion
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    PromotionCode = entity.Code,
                    ProductCode = productCode
                });
            }
        }

        await _promotionRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var resultDto = _mapper.Map<PromotionDto>(entity);
        resultDto.ProductCodes = dto.ProductCodes; 
        
        return Result<PromotionDto>.Success(resultDto);
    }

    public async Task<Result<PromotionDto>> Handle(UpdateCommand<UpdatePromotionDto, PromotionDto> request, CancellationToken cancellationToken)
    {
        var entity = await _promotionRepository.GetByCodeAsync(request.Code, cancellationToken);
        
        if (entity == null)
            return Result<PromotionDto>.Failure(Error.NotFound("Promotion.NotFound", $"Promotion with code {request.Code} not found"));

        _mapper.Map(request.Dto, entity);

        // Sync ProductCodes
        if (request.Dto.ProductCodes != null)
        {
            // Clear existing links
            entity.TblProductPromotions.Clear();

            // Add new links
            foreach (var productCode in request.Dto.ProductCodes)
            {
                entity.TblProductPromotions.Add(new TblProductPromotion
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    PromotionCode = entity.Code,
                    ProductCode = productCode
                });
            }
        }

        _promotionRepository.Update(entity);
        await _unitOfWork.CommitAsync(cancellationToken);

        var resultDto = _mapper.Map<PromotionDto>(entity);
        if (request.Dto.ProductCodes != null)
        {
            resultDto.ProductCodes = request.Dto.ProductCodes;
        }

        return Result<PromotionDto>.Success(resultDto);
    }
}
