using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Application;
using Web.Entity;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsController(INewsRepository newsRepository) : BaseController {
  [HttpGet("/v1/article/{id}")]
  public async Task<ActionResult<NewsArticleDto>> GetArticle(int id) {
    var result = await newsRepository.GetArticleAsync(id);
    if (result == null) return NotFound();
    return OkResult(result);
  }
  
  [HttpGet("/v1/articles")]
  public async Task<ActionResult<IEnumerable<NewsArticleDto>>> GetArticles() {
    var result = await newsRepository.GetArticlesAsync();
    return OkResult(result);
  }
  
  [HttpPost("/v1/article")]
  public async Task<ActionResult<NewsArticleDto>> CreateArticle(
    [FromBody] ArticleCreateRequest article
    ) {
    var newsArticleCreateDto = new NewsArticleCreateDto {
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      PublicationDate = DateTime.UtcNow,
    };
    
    var result = await newsRepository.CreateArticleAsync(newsArticleCreateDto);
    return OkResult(result);
  }
  
  [HttpPatch("/v1/article")]
  public async Task<ActionResult<NewsArticleDto>> UpdateArticle(
    [FromBody] ArticleUpdateRequest article
  ) {
    var newsArticleCreateDto = new NewsArticleUpdateDto {
      Id = article.Id,
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      PublicationDate = DateTime.UtcNow,
    };
    
    var result = await newsRepository.UpdateArticleAsync(newsArticleCreateDto);
    return OkResult(result);
  }
  
  [HttpDelete("/v1/article/{id}")]
  public async Task<ActionResult> DeleteArticle(
    int id
  ) {
    await newsRepository.DeleteArticleAsync(id);
    return Ok();
  }
}