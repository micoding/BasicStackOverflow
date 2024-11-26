using BasicStackOverflow.Entities;
using FluentValidation;

namespace BasicStackOverflow.Models.Validators;

public class PagedQueryValidator : AbstractValidator<PagesQuery>
{
    private readonly int?[] allowedPageSizes = { 5, 10, 20, 50 };

    private readonly string[] allowedSortBy =
        { nameof(Question.Title), nameof(Question.Author), nameof(Question.CreationDate), nameof(Question.AuthorId) };

    public PagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).NotEmpty().GreaterThan(0);
        RuleFor(x => x.PageSize).Custom((value, context) =>
        {
            if (!allowedPageSizes.Contains(value))
                context.AddFailure("PageSize",
                    $"Page size is invalid, must be in {string.Join(",", allowedPageSizes)}.");
        });
        RuleFor(x => x.SortBy).Must(x => string.IsNullOrEmpty(x) || allowedSortBy.Contains(x))
            .WithMessage($"Sort is optional, must be in: {string.Join(", ", allowedSortBy)}.");

        When(x => x.SortBy != null, () => { RuleFor(x => x.SortAscending).NotNull(); });
    }
}