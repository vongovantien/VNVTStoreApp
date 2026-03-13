using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Products.Commands;

namespace VNVTStore.Application.Products.Handlers;

public class CreateQuestionHandler : IRequestHandler<CreateQuestionCommand, Result<bool>>
{
    private readonly IRepository<TblReview> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateQuestionHandler(
        IRepository<TblReview> repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.UserCode))
        {
            return Result.Failure<bool>(Error.Unauthorized());
        }

        var question = new TblReview
        {
            Code = "Q" + DateTime.Now.Ticks.ToString().Substring(10),
            ProductCode = request.ProductCode,
            UserCode = _currentUser.UserCode,
            Comment = request.Question,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false, // Must be approved by admin
            IsActive = true,
            OrderItemCode = null // Distinction from "Review"
        };

        await _repository.AddAsync(question, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
