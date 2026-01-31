using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using AutoMapper;

namespace VNVTStore.Application.Products.Handlers;

public class DeleteProductHandler : BaseHandler<TblProduct>,
    IRequestHandler<DeleteCommand<TblProduct>, Result>
{
    private readonly IFileService _fileService;
    private readonly IApplicationDbContext _context;

    public DeleteProductHandler(
        IRepository<TblProduct> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IFileService fileService,
        IApplicationDbContext context)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
        _fileService = fileService;
        _context = context;
    }

    public async Task<Result> Handle(DeleteCommand<TblProduct> request, CancellationToken cancellationToken)
    {

        var result = await DeleteAsync(request.Code, MessageConstants.Product, cancellationToken);

        if (result.IsSuccess)
        {
            await _fileService.DeleteLinkedFilesAsync(request.Code, "Product", cancellationToken);
        }

        return result;
    }
}
