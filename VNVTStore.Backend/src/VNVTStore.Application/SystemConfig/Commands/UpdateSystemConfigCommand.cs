using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.SystemConfig.Commands
{
    public class UpdateSystemConfigCommand : IRequest<Result<SystemConfigDto>>
    {
        public string ConfigKey { get; set; } = null!;
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateSystemConfigCommandHandler : IRequestHandler<UpdateSystemConfigCommand, Result<SystemConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UpdateSystemConfigCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<SystemConfigDto>> Handle(UpdateSystemConfigCommand request, CancellationToken cancellationToken)
        {
            var config = await _context.TblSystemConfigs
                .FirstOrDefaultAsync(c => c.Code == request.ConfigKey, cancellationToken);

            if (config == null)
            {
                config = new TblSystemConfig
                {
                    Code = request.ConfigKey,
                    ConfigValue = request.ConfigValue,
                    Description = request.Description,
                    IsActive = request.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ModifiedType = "ADD"
                };
                _context.TblSystemConfigs.Add(config);
            }
            else
            {
                if (request.ConfigValue != null) config.ConfigValue = request.ConfigValue;
                if (request.Description != null) config.Description = request.Description;
                if (request.IsActive.HasValue) config.IsActive = request.IsActive.Value;
                config.UpdatedAt = DateTime.UtcNow;
                config.ModifiedType = "UPDATE";
            }

            await _context.SaveChangesAsync(cancellationToken);

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
