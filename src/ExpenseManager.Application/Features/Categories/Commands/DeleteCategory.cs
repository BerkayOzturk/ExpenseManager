using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest;

public sealed class DeleteCategoryHandler(ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var category = await categories.GetByIdAsync(userId, request.Id, cancellationToken);
        if (category is null) throw new NotFoundException("Category not found.");

        categories.Remove(category);
        await uow.SaveChangesAsync(cancellationToken);
    }
}

