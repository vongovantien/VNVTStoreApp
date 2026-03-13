using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Commands;
using VNVTStore.Application.Common.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Common.Settings;

public class ConfigHandlers : 
    IRequestHandler<GetShopConfigsQuery, Result<List<ShopConfigDto>>>,
    IRequestHandler<GetConfigByCodeQuery, Result<ShopConfigDto>>,
    IRequestHandler<UpdateConfigCommand, Result<ShopConfigDto>>
{
    private readonly IRepository<TblSystemConfig> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ConfigHandlers(IRepository<TblSystemConfig> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ShopConfigDto>>> Handle(GetShopConfigsQuery request, CancellationToken cancellationToken)
    {
        var configs = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(_mapper.Map<List<ShopConfigDto>>(configs));
    }

    public async Task<Result<ShopConfigDto>> Handle(GetConfigByCodeQuery request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (config == null) return Result.Failure<ShopConfigDto>("Config not found");
        return Result.Success(_mapper.Map<ShopConfigDto>(config));
    }

    public async Task<Result<ShopConfigDto>> Handle(UpdateConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (config == null) return Result.Failure<ShopConfigDto>("Config not found");

        config.ConfigValue = request.Dto.ConfigValue;
        config.Description = request.Dto.Description;
        config.UpdatedAt = DateTime.UtcNow;

        _repository.Update(config);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<ShopConfigDto>(config));
    }
}
