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
                // Feature: Default values for critical configurations if not yet defined in DB
                if (request.ConfigKey == "FLASHSALE_TIMES")
                {
                    return Result<SystemConfigDto>.Success(new SystemConfigDto
                    {
                        ConfigKey = "FLASHSALE_TIMES",
                        ConfigValue = "[{\"active\":true,\"label\":\"09:00\"},{\"active\":true,\"label\":\"12:00\"},{\"active\":true,\"label\":\"15:00\"},{\"active\":true,\"label\":\"18:00\"},{\"active\":true,\"label\":\"21:00\"}]",
                        Description = "Default Flash Sale Schedule",
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                return Result<SystemConfigDto>.Failure(Error.NotFound("SystemConfig.NotFound", $"Configuration '{request.ConfigKey}' not found"));
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
