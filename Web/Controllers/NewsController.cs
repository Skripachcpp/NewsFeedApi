using Domain.DTOs;
using Domain.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Application;
using Web.Entity;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class NewsController(
    INewsRepository newsRepository,
    ICacheRepository cacheRepository): BaseController
{
    [HttpGet("article/{id}")]
    public async Task<ActionResult<NewsArticleDto>> GetArticle(int id, CancellationToken cancellationToken = default)
    {
        var result = await newsRepository.GetArticleAsync(id, cancellationToken).ConfigureAwait(false);
        if (result == null) return this.NotFound("статья не найдена");

        return result;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<PageDto<NewsArticleDto>>> GetArticles(
        int offset = 0,
        int count = 100,
        CancellationToken cancellationToken = default)
    {
        if (offset < 0) offset = 0;
        if (count < 0) count = 100;
        if (count > 1000) return this.BadRequest("count не может быть больше 1000");

        // ну не знаю, глаза ломаются, возможно если управлять кешем вручную это будет легче читаться
        var page = await cacheRepository.AutoCacheAsync(
            CacheKeys.Articles,
            [offset.ToStr(), count.ToStr()],
            async () => await newsRepository.GetArticlesAsync(offset, count, cancellationToken)
                .ConfigureAwait(false)).ConfigureAwait(false);

        return page;
    }

    [Authorize]
    [HttpPost("article")]
    public async Task<ActionResult<NewsArticleDto>> CreateArticle(
        [FromBody] ArticleCreateRequest article,
        CancellationToken cancellationToken = default)
    {
        var userInfo = this.GetUserInfo();
        if (userInfo == null) return this.BadRequest("не удалось получить данные о пользователе");

        var result = await newsRepository.CreateArticleAsync(
            new NewsArticleCreateDto
            {
                Title = article.Title,
                Content = article.Content,
                Summary = article.Summary,
                Tags = article.Tags,
                UserId = userInfo.id,
                UserName = userInfo.name,
            }, cancellationToken).ConfigureAwait(false);

        await cacheRepository.Clear(CacheKeys.Articles).ConfigureAwait(false);
        return result;
    }

    [Authorize]
    [HttpPatch("article")]
    public async Task<ActionResult<NewsArticleDto>> UpdateArticle(
        [FromBody] ArticleUpdateRequest article,
        CancellationToken cancellationToken = default)
    {
        var userInfo = this.GetUserInfo();
        if (userInfo == null) return this.BadRequest("не удалось получить данные о пользователе");

        var result = await newsRepository.UpdateArticleAsync(
            new NewsArticleUpdateDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Summary = article.Summary,
                Tags = article.Tags ?? [],
                UserId = userInfo.id, // теперь это его статья
                UserName = userInfo.name,
            }, cancellationToken).ConfigureAwait(false);

        if (result == null) return this.NotFound("статья не найдена");

        await cacheRepository.Clear(CacheKeys.Articles).ConfigureAwait(false);
        return result;
    }

    [Authorize]
    [HttpDelete("article/{id}")]
    public async Task<ActionResult> DeleteArticle(
        int id,
        CancellationToken cancellationToken = default)
    {
        var success = await newsRepository.DeleteArticleAsync(id, cancellationToken).ConfigureAwait(false);
        if (success == false) return this.NotFound("статья не найдена");

        await cacheRepository.Clear(CacheKeys.Articles).ConfigureAwait(false);
        return this.Ok();
    }
}
