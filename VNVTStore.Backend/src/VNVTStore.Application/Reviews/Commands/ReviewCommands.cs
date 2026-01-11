using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Reviews.Commands;

public record ApproveReviewCommand(string Code) : IRequest<Result>;
public record RejectReviewCommand(string Code) : IRequest<Result>;
