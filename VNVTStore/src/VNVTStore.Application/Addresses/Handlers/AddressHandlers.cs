using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Addresses.Commands;
using VNVTStore.Application.Addresses.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Addresses.Handlers;

public class AddressHandlers :
    IRequestHandler<CreateAddressCommand, Result<AddressDto>>,
    IRequestHandler<UpdateAddressCommand, Result<AddressDto>>,
    IRequestHandler<DeleteAddressCommand, Result<bool>>,
    IRequestHandler<SetDefaultAddressCommand, Result<bool>>,
    IRequestHandler<GetUserAddressesQuery, Result<IEnumerable<AddressDto>>>,
    IRequestHandler<GetAddressByCodeQuery, Result<AddressDto>>
{
    private readonly IRepository<TblAddress> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddressHandlers(
        IRepository<TblAddress> addressRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AddressDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        // If setting as default, reset others
        if (request.IsDefault)
        {
            var existingAddresses = await _addressRepository.FindAllAsync(
                a => a.UserCode == request.UserCode && a.IsDefault == true, cancellationToken);
            foreach (var addr in existingAddresses)
            {
                addr.IsDefault = false;
                _addressRepository.Update(addr);
            }
        }

        var address = new TblAddress
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = request.UserCode,
            AddressLine = request.AddressLine,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country ?? "Vietnam",
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        await _addressRepository.AddAsync(address, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<AddressDto>(address));
    }

    public async Task<Result<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByCodeAsync(request.AddressCode, cancellationToken);

        if (address == null)
            return Result.Failure<AddressDto>(Error.NotFound("Address", request.AddressCode));

        if (address.UserCode != request.UserCode)
            return Result.Failure<AddressDto>(Error.Forbidden("Cannot update another user's address"));

        if (request.AddressLine != null) address.AddressLine = request.AddressLine;
        if (request.City != null) address.City = request.City;
        if (request.State != null) address.State = request.State;
        if (request.PostalCode != null) address.PostalCode = request.PostalCode;
        
        if (request.IsDefault == true)
        {
            var existingAddresses = await _addressRepository.FindAllAsync(
                a => a.UserCode == request.UserCode && a.IsDefault == true && a.Code != request.AddressCode, cancellationToken);
            foreach (var addr in existingAddresses)
            {
                addr.IsDefault = false;
                _addressRepository.Update(addr);
            }
            address.IsDefault = true;
        }

        _addressRepository.Update(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<AddressDto>(address));
    }

    public async Task<Result<bool>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByCodeAsync(request.AddressCode, cancellationToken);

        if (address == null)
            return Result.Failure<bool>(Error.NotFound("Address", request.AddressCode));

        if (address.UserCode != request.UserCode)
            return Result.Failure<bool>(Error.Forbidden("Cannot delete another user's address"));

        _addressRepository.Delete(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByCodeAsync(request.AddressCode, cancellationToken);

        if (address == null)
            return Result.Failure<bool>(Error.NotFound("Address", request.AddressCode));

        if (address.UserCode != request.UserCode)
            return Result.Failure<bool>(Error.Forbidden("Cannot modify another user's address"));

        // Reset all other addresses
        var existingAddresses = await _addressRepository.FindAllAsync(
            a => a.UserCode == request.UserCode && a.IsDefault == true, cancellationToken);
        foreach (var addr in existingAddresses)
        {
            addr.IsDefault = false;
            _addressRepository.Update(addr);
        }

        address.IsDefault = true;
        _addressRepository.Update(address);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<IEnumerable<AddressDto>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _addressRepository.AsQueryable()
            .Where(a => a.UserCode == request.UserCode)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<AddressDto>>(addresses));
    }

    public async Task<Result<AddressDto>> Handle(GetAddressByCodeQuery request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByCodeAsync(request.AddressCode, cancellationToken);

        if (address == null)
            return Result.Failure<AddressDto>(Error.NotFound("Address", request.AddressCode));

        return Result.Success(_mapper.Map<AddressDto>(address));
    }
}
