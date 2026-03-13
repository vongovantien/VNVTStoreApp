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

public class AddressHandlers : BaseHandler<TblAddress, AddressDto, CreateAddressDto, UpdateAddressDto>,
    IRequestHandler<SetDefaultAddressCommand, Result>,
    IRequestHandler<GetAllQuery<AddressDto>, Result<IEnumerable<AddressDto>>>
{
    private readonly ICurrentUser _currentUser;
    // _mobileService? No, just currentUser, unitOfWork, mapper, repository are in base.
    // _addressRepository is in base as _repository (TblAddress).
    
    public AddressHandlers(
        IRepository<TblAddress> repository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _currentUser = currentUser;
    }

    public override async Task<Result<AddressDto>> Handle(CreateCommand<CreateAddressDto, AddressDto> request, CancellationToken cancellationToken)
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
            return Result.Failure<AddressDto>(Error.Unauthorized());

        // If setting as default, reset others
        if (request.Dto.IsDefault)
        {
            await ResetOtherDefaultsInternal(userCode, null, cancellationToken);
        }

        var address = new TblAddress.Builder()
            .WithUser(userCode)
            .AtLocation(
                request.Dto.AddressLine!,
                request.Dto.FullName,
                request.Dto.Phone,
                request.Dto.Category,
                request.Dto.City,
                request.Dto.State,
                request.Dto.PostalCode,
                request.Dto.Country)
            .Build();

        if (request.Dto.IsDefault)
        {
             address.SetAsDefault();
        }

        await _repository.AddAsync(address, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<AddressDto>(address));
    }

    public override async Task<Result<AddressDto>> Handle(UpdateCommand<UpdateAddressDto, AddressDto> request, CancellationToken cancellationToken)
    {
        var address = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure<AddressDto>(Error.NotFound(MessageConstants.Address, request.Code));

        var userCode = _currentUser.UserCode;
        if (address.UserCode != userCode)
            return Result.Failure<AddressDto>(Error.Forbidden("Cannot update another user's address"));

        if (request.Dto.IsDefault == true)
        {
            await ResetOtherDefaultsInternal(userCode!, request.Code, cancellationToken);
        }

        address.Update(
            new AddressDetails(
                request.Dto.AddressLine ?? address.AddressLine,
                request.Dto.FullName ?? address.FullName,
                request.Dto.Phone ?? address.Phone,
                request.Dto.Category ?? address.Category,
                request.Dto.City ?? address.City,
                request.Dto.State ?? address.State,
                request.Dto.PostalCode ?? address.PostalCode,
                request.Dto.Country ?? address.Country
            ),
            request.Dto.IsDefault
        );

        _repository.Update(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<AddressDto>(address));
    }

    public override async Task<Result> Handle(DeleteCommand<TblAddress> request, CancellationToken cancellationToken)
    {
        var address = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure(Error.NotFound(MessageConstants.Address, request.Code));

        var userCode = _currentUser.UserCode;
        if (address.UserCode != userCode)
            return Result.Failure(Error.Forbidden("Cannot delete another user's address"));

        _repository.Delete(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure(Error.NotFound(MessageConstants.Address, request.Code));

        if (address.UserCode != _currentUser.UserCode)
            return Result.Failure(Error.Forbidden("Cannot modify another user's address"));

        await ResetOtherDefaultsInternal(_currentUser.UserCode!, request.Code, cancellationToken);

        address.SetAsDefault();
        _repository.Update(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<AddressDto>>> Handle(GetAllQuery<AddressDto> request, CancellationToken cancellationToken)
    {
        var addresses = await _repository.AsQueryable()
            .Where(a => a.UserCode == _currentUser.UserCode)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<AddressDto>>(addresses));
    }

    public override async Task<Result<AddressDto>> Handle(GetByCodeQuery<AddressDto> request, CancellationToken cancellationToken)
    {
        var address = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (address == null)
            return Result.Failure<AddressDto>(Error.NotFound(MessageConstants.Address, request.Code));

        if (address.UserCode != _currentUser.UserCode)
            return Result.Failure<AddressDto>(Error.Forbidden("Cannot view another user's address"));

        return Result.Success(_mapper.Map<AddressDto>(address));
    }

    private async Task ResetOtherDefaultsInternal(string userCode, string? currentAddressCode, CancellationToken cancellationToken)
    {
        var existingAddresses = await _repository.FindAllAsync(
            a => a.UserCode == userCode && a.IsDefault == true && (currentAddressCode == null || a.Code != currentAddressCode), 
            cancellationToken);
            
        foreach (var addr in existingAddresses)
        {
            addr.UnsetDefault();
            _repository.Update(addr);
        }
    }
}
