using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using AutoMapper;

namespace VNVTStore.Application.Auth.Commands;

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result<AuthResponseDto>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(
        IRepository<TblUser> repository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Invalid access token or refresh token"));
        }

        var username = principal.Identity?.Name;
        // Or find by userCode claim
        var userCodeClaim = principal.Claims.FirstOrDefault(c => c.Type == "userCode");
        var userCode = userCodeClaim?.Value;

        if (string.IsNullOrEmpty(userCode))
        {
             return Result.Failure<AuthResponseDto>(Error.Validation("Invalid token claims"));
        }

        var user = await _repository.FindAsync(u => u.Code == userCode, cancellationToken);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Invalid access token or refresh token"));
        }

        var newAccessToken = _jwtService.GenerateToken(user.Code, user.Username, user.Email, user.Role);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = _mapper.Map<UserDto>(user)
        });
    }
}
