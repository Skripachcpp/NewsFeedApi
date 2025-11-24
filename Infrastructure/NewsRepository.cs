using System.Data;
using Dapper;
using Domain.DTOs;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure;

public class NewsRepository(DpContext dpContext): INewsRepository
{
    // language=PostgreSQL
    private const string BaseSelectQuerySelect = @"
       SELECT
        na.id as Id,
        na.title as Title,
        na.content as Content,
        na.summary as Summary,
        na.publication_date as PublicationDate,
        na.user_name as UserName,
        COALESCE(array_agg(t.name) FILTER (WHERE t.name IS NOT NULL), ARRAY[]::text[]) as Tags
      FROM news_article na 
      LEFT JOIN news_article_tag nat ON na.id = nat.news_article_id
      LEFT JOIN tag t ON nat.tag_id = t.id
      ";

    private const string BaseSelectQueryEnding = @"
      GROUP BY na.id, na.title, na.content, na.summary, na.publication_date, na.user_name
      ORDER BY na.publication_date DESC
      ";

    public async Task<PageDto<NewsArticleDto>> GetArticlesAsync(int offset = 0, int count = 100, CancellationToken cancellationToken = default)
    {
        using var connection = dpContext.OpenConnection();

        // language=PostgreSQL
        var page = await dpContext.PageAsync<NewsArticleDto>(
            $@"
                SELECT
                  na.id as Id,
                  na.title as Title,
                  na.content as Content,
                  na.summary as Summary,
                  na.publication_date as PublicationDate,
                  na.user_name as UserName,
                  COALESCE(
                    (SELECT array_agg(t.name ORDER BY t.name)
                     FROM news_article_tag nat
                     INNER JOIN tag t ON nat.tag_id = t.id
                     WHERE nat.news_article_id = na.id),
                    ARRAY[]::text[]
                  ) as Tags
                FROM (
                  SELECT id, title, content, summary, publication_date, user_name
                  FROM news_article
                  ORDER BY publication_date DESC
                  LIMIT @Count OFFSET @Offset
                ) na
                ORDER BY na.publication_date DESC
              ",

            // language=PostgreSQL
            @"SELECT COUNT(*) as cnt FROM news_article",
            parameters: new { Count = count, Offset = offset },
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return page;
    }

    private async Task<NewsArticleDto?> QueryGetArticleAsync(
        int id,
        IDbConnection connection,
        CancellationToken cancellationToken = default,
        IDbTransaction? transaction = default)
    {
        // language=PostgreSQL
        var result = await connection.QueryFirstOrDefaultAsync<NewsArticleDto>(new CommandDefinition(
            $@"{BaseSelectQuerySelect}
        WHERE na.id = @Id
        {BaseSelectQueryEnding}",
            parameters: new { Id = id },
            cancellationToken: cancellationToken,
            transaction: transaction)).ConfigureAwait(false);

        return result;
    }

    public async Task<NewsArticleDto?> GetArticleAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = dpContext.OpenConnection();
        var result = await this.QueryGetArticleAsync(id, connection, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task QueryCreateIfNotExistsTagAndAttachToArticleAsync(
        IEnumerable<string> tagNames,
        int articleId,
        IDbConnection connection,
        CancellationToken cancellationToken = default,
        IDbTransaction? transaction = default)
    {
        var tagNamesList = tagNames.ToList();
        if (tagNamesList.Count <= 0)
            return;

        // language=PostgreSQL
        var tagIdsList = (await connection.QueryAsync<int>(new CommandDefinition(
            @"
                INSERT INTO tag (name)
                SELECT unnest(@Names)
                ON CONFLICT (name) DO NOTHING;

                SELECT id FROM tag
                WHERE name = ANY(@Names)
              ",
            parameters: new { Names = tagNamesList },
            transaction: transaction,
            cancellationToken: cancellationToken)).ConfigureAwait(false)).ToList();

        if (tagIdsList.Count <= 0)
            return;

        // language=PostgreSQL
        await connection.ExecuteAsync(new CommandDefinition(
            @"
            DELETE FROM news_article_tag WHERE news_article_id = @ArticleId;

            INSERT INTO news_article_tag (news_article_id, tag_id)
            SELECT @ArticleId, unnest(@TagIds)
            ON CONFLICT (news_article_id, tag_id) DO NOTHING;           
          ",
            parameters: new { ArticleId = articleId, TagIds = tagIdsList },
            cancellationToken: cancellationToken,
            transaction: transaction)).ConfigureAwait(false);
    }

    public async Task<NewsArticleDto> CreateArticleAsync(
        NewsArticleCreateDto article,
        CancellationToken cancellationToken = default)
    {
        if (article == null) throw new ArgumentNullException(nameof(article));

        using var connection = dpContext.OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // language=PostgreSQL
            var articleId = await connection.QuerySingleAsync<int>(new CommandDefinition(
                @"
                INSERT INTO news_article (title, content, summary, publication_date, user_id, user_name)
                VALUES (@Title, @Content, @Summary, @PublicationDate, @UserId, @UserName)
                RETURNING id
                ",
                parameters: new
                {
                    Title = article.Title,
                    Content = article.Content,
                    Summary = article.Summary,
                    PublicationDate = DateTime.UtcNow,
                    UserId = article.UserId,
                    UserName = article.UserName,
                },
                transaction: transaction,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            await this.QueryCreateIfNotExistsTagAndAttachToArticleAsync(
                article.Tags ?? [],
                articleId,
                connection: connection,
                cancellationToken: cancellationToken,
                transaction: transaction).ConfigureAwait(false);

            var articleNext = await this.QueryGetArticleAsync(articleId, connection, cancellationToken, transaction).ConfigureAwait(false);
            if (articleNext == null)
            {
                throw new InvalidOperationException("Не удалось загрузить созданную статью");
            }

            transaction.Commit();

            return articleNext;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<NewsArticleDto?> UpdateArticleAsync(
        NewsArticleUpdateDto article,
        CancellationToken cancellationToken = default)
    {
        if (article == null) throw new ArgumentNullException(nameof(article));

        using var connection = dpContext.OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // language=PostgreSQL
            var articleId = await connection.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
                @"
        UPDATE news_article
        SET title = @Title,
            content = @Content,
            summary = @Summary,
            publication_date = @PublicationDate,
            user_id = @UserId,
            user_name = @UserName
        WHERE id = @Id
        RETURNING id
        ",
                parameters: new
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Summary = article.Summary,

                    // пусть дата обновляется при изменении стати
                    PublicationDate = DateTime.UtcNow,
                    UserId = article.UserId,
                    UserName = article.UserName,
                },
                transaction: transaction,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            if (articleId == null)
            {
                return null;
            }

            await this.QueryCreateIfNotExistsTagAndAttachToArticleAsync(
                article.Tags ?? [],
                articleId.Value,
                connection: connection,
                cancellationToken: cancellationToken,
                transaction: transaction).ConfigureAwait(false);

            var articleNext = await this.QueryGetArticleAsync(articleId.Value, connection, cancellationToken, transaction).ConfigureAwait(false);
            if (articleNext == null)
            {
                throw new InvalidOperationException("Не удалось найти обновленную статью");
            }

            transaction.Commit();

            return articleNext;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id, CancellationToken cancellationToken = default)
    {
        // каскадное удаление
        // language=PostgreSQL
        var rowsAffected = await dpContext.ExecuteWithTransactionAsync(
            @"DELETE FROM news_article WHERE id = @Id",
            parameters: new { Id = id },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return rowsAffected > 0;
    }
}
