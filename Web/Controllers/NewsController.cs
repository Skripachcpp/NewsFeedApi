using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Entity;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsController(INewsRepository newsRepository) : ControllerBase {
  [HttpGet("/v1/article/{id}")]
  public async Task<ActionResult<IEnumerable<NewsArticle>>> GetArticle(int id) {
    var result = await newsRepository.GetArticleAsync(id);
    return Ok(result);
  }
  
  [HttpGet("/v1/article")]
  public async Task<ActionResult<IEnumerable<NewsArticle>>> GetArticles() {
    var result = await newsRepository.GetArticlesAsync();
    return Ok(result);
  }
  
  [HttpPost("/v1/article")]
  public async Task<ActionResult<NewsArticleDto>> CreateArticle(
    [FromBody] ArticleCreateRequest article
    ) {

    var newsArticleCreateDto = new NewsArticleCreateDto {
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      PublicationDate = DateTime.UtcNow,
    };
    
    var result = await newsRepository.CreateArticleAsync(newsArticleCreateDto);
    return Ok(result);
  }
}