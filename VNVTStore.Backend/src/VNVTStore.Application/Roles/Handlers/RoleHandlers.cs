using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Roles.Handlers;

public class RoleHandlers : BaseHandler<TblRole, RoleDto, CreateRoleDto, UpdateRoleDto>
{
    private readonly IApplicationDbContext _context;

    public RoleHandlers(
        IRepository<TblRole> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IDapperContext dapperContext,
        IApplicationDbContext context) 
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _context = context;
    }

    public override async Task<Result<RoleDto>> Handle(CreateCommand<CreateRoleDto, RoleDto> request, CancellationToken cancellationToken)
    {
         return await base.CreateAsync<CreateRoleDto, RoleDto>(request.Dto, cancellationToken, entity => {
             if (string.IsNullOrEmpty(entity.Code))
             {
                 entity.Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
             }
             
             // Handle Permissions
             if (request.Dto.PermissionCodes?.Any() == true)
             {
                 foreach (var permCode in request.Dto.PermissionCodes)
                 {
                     entity.TblRolePermissions.Add(new TblRolePermission { RoleCode = entity.Code, PermissionCode = permCode });
                 }
             }
             
             // Handle Menus
             if (request.Dto.MenuCodes?.Any() == true)
             {
                 foreach (var menuCode in request.Dto.MenuCodes)
                 {
                     entity.TblRoleMenus.Add(new TblRoleMenu { RoleCode = entity.Code, MenuCode = menuCode });
                 }
             }
         });
    }

    public override async Task<Result<RoleDto>> Handle(UpdateCommand<UpdateRoleDto, RoleDto> request, CancellationToken cancellationToken)
    {
        return await base.UpdateAsync<UpdateRoleDto, RoleDto>(request.Code, request.Dto, "Role", cancellationToken, async entity => {
            // Handle Permissions
            if (request.Dto.PermissionCodes != null)
            {
                var existingPerms = await _context.TblRolePermissions
                    .Where(rp => rp.RoleCode == entity.Code)
                    .ToListAsync(cancellationToken);
                
                _context.TblRolePermissions.RemoveRange(existingPerms);

                foreach (var permCode in request.Dto.PermissionCodes)
                {
                    entity.TblRolePermissions.Add(new TblRolePermission { RoleCode = entity.Code, PermissionCode = permCode });
                }
            }
            
            // Handle Menus
            if (request.Dto.MenuCodes != null)
            {
                var existingMenus = await _context.TblRoleMenus
                    .Where(rm => rm.RoleCode == entity.Code)
                    .ToListAsync(cancellationToken);
                
                _context.TblRoleMenus.RemoveRange(existingMenus);

                foreach (var menuCode in request.Dto.MenuCodes)
                {
                    entity.TblRoleMenus.Add(new TblRoleMenu { RoleCode = entity.Code, MenuCode = menuCode });
                }
            }
        });
    }

    public override async Task<Result<RoleDto>> Handle(GetByCodeQuery<RoleDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeIncludeChildrenAsync<RoleDto>(request.Code, "Role", cancellationToken, query => 
            query.Include(r => r.TblRolePermissions)
                 .ThenInclude(rp => rp.PermissionCodeNavigation)
                 .Include(r => r.TblRoleMenus)
                 .ThenInclude(rm => rm.MenuCodeNavigation));
    }
}
