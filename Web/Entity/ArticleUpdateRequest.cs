using Web.Application;

namespace Web.Entity;

public sealed record ArticleUpdateRequest : ArticleCreateRequest
{
    [Validate(Min = 0, ErrorMessage = "обязательно")]
    public int Id { get; init; } = -1;
}