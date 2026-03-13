using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Commands;

public record CreateQuestionCommand(string ProductCode, string Question) : IRequest<Result<bool>>;

public record ApproveQuestionCommand(string QuestionCode) : IRequest<Result<bool>>;

public record AnswerQuestionCommand(string QuestionCode, string Answer) : IRequest<Result<bool>>;
