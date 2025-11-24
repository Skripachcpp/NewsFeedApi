using Domain.DTOs;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class TagsRepository(DpContext dpContext): ITagsRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<TagDto>> GetTags(CancellationToken cancellationToken = default)
    {
        // language=PostgreSQL
        var result = await dpContext.QueryAsync<TagDto>(
            @"SELECT id as Id, name as Name FROM tag",
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public async Task DeleteTag(int id, CancellationToken cancellationToken = default)
    {
        // каскадное удаление, тут нужна транзакция
        // language=PostgreSQL
        await dpContext.ExecuteWithTransactionAsync(
            @"DELETE FROM tag WHERE id = @Id",
            parameters: new { Id = id },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
