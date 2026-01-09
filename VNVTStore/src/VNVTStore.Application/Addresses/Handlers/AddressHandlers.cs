using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Addresses.Commands;
using VNVTStore.Application.Addresses.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Addresses.Handlers;

public class AddressHandlers : BaseHandler<TblAddress>,
    IRequestHandler<CreateCommand<CreateAddressDto, AddressDto>, Result<AddressDto>>,
    IRequestHandler<UpdateCommand<UpdateAddressDto, AddressDto>, Result<AddressDto>>,
    IRequestHandler<DeleteCommand<TblAddress>, Result>,
    IRequestHandler<SetDefaultAddressCommand, Result>,
    IRequestHandler<GetAllQuery<AddressDto>, Result<IEnumerable<AddressDto>>>,
    IRequestHandler<GetByCodeQuery<AddressDto>, Result<AddressDto>>
{
    private readonly ICurrentUser _currentUser;

    public AddressHandlers(
        IRepository<TblAddress> addressRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(addressRepository, unitOfWork, mapper)
    {
        _currentUser = currentUser;
    }

    public async Task<Result<AddressDto>> Handle(CreateCommand<CreateAddressDto, AddressDto> request, CancellationToken cancellationToken)
    {
        // If setting as default, reset others
        if (request.Dto.IsDefault)
        {
            await ResetOtherDefaultsInternal(_currentUser.UserCode!, null, cancellationToken);
        }

        return await CreateAsync<CreateAddressDto, AddressDto>(
            request.Dto,
            cancellationToken,
            a => {
                a.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                a.CreatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(a.Country)) a.Country = "Vietnam";
            });
    }

    public async Task<Result<AddressDto>> Handle(UpdateCommand<UpdateAddressDto, AddressDto> request, CancellationToken cancellationToken)
    {
        var address = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure<AddressDto>(Error.NotFound(MessageConstants.Address, request.Code));

        var userCode = _currentUser.UserCode;
        if (address.UserCode != userCode)
            return Result.Failure<AddressDto>(Error.Forbidden("Cannot update another user's address"));

        if (request.Dto.IsDefault == true)
        {
            await ResetOtherDefaultsInternal(userCode!, request.Code, cancellationToken);
        }

        return await UpdateAsync<UpdateAddressDto, AddressDto>(
            request.Code,
            request.Dto,
            MessageConstants.Address,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblAddress> request, CancellationToken cancellationToken)
    {
        var address = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure(Error.NotFound(MessageConstants.Address, request.Code));

        var userCode = _currentUser.UserCode;
        if (address.UserCode != userCode)
            return Result.Failure(Error.Forbidden("Cannot delete another user's address"));

        return await DeleteAsync(request.Code, MessageConstants.Address, cancellationToken, softDelete: false);
    }

    public async Task<Result> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure(Error.NotFound(MessageConstants.Address, request.Code));

        if (address.UserCode != _currentUser.UserCode)
            return Result.Failure(Error.Forbidden("Cannot modify another user's address"));

        await ResetOtherDefaultsInternal(_currentUser.UserCode!, request.Code, cancellationToken);

        address.IsDefault = true;
        Repository.Update(address);
        await UnitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<AddressDto>>> Handle(GetAllQuery<AddressDto> request, CancellationToken cancellationToken)
    {
        var addresses = await Repository.AsQueryable()
            .Where(a => a.UserCode == _currentUser.UserCode)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(Mapper.Map<IEnumerable<AddressDto>>(addresses));
    }

    public async Task<Result<AddressDto>> Handle(GetByCodeQuery<AddressDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<AddressDto>(request.Code, MessageConstants.Address, cancellationToken);
    }

    private async Task ResetOtherDefaultsInternal(string userCode, string? currentAddressCode, CancellationToken cancellationToken)
    {
        var existingAddresses = await Repository.FindAllAsync(
            a => a.UserCode == userCode && a.IsDefault == true && (currentAddressCode == null || a.Code != currentAddressCode), 
            cancellationToken);
            
        foreach (var addr in existingAddresses)
        {
            addr.IsDefault = false;
            Repository.Update(addr);
        }
    }
}
