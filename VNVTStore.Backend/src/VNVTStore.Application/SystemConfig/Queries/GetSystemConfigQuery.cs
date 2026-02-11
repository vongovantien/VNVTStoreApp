using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace VNVTStore.Application.SystemConfig.Queries
{
    public class GetSystemConfigQuery : IRequest<Result<SystemConfigDto>>
    {
        public string ConfigKey { get; set; } = null!;
    }

    public class GetSystemConfigQueryHandler : IRequestHandler<GetSystemConfigQuery, Result<SystemConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSystemConfigQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<SystemConfigDto>> Handle(GetSystemConfigQuery request, CancellationToken cancellationToken)
        {
            var config = await _context.TblSystemConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == request.ConfigKey, cancellationToken);

            if (config == null)
            {
                return Result<SystemConfigDto>.Failure("Configuration not found");
            }

            var dto = new SystemConfigDto
            {
                ConfigKey = config.Code,
                ConfigValue = config.ConfigValue,
                Description = config.Description,
                IsActive = config.IsActive,
                UpdatedAt = config.UpdatedAt
            };

            return Result<SystemConfigDto>.Success(dto);
        }
    }
}
