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
  public async Task<ActionResult<NewsArticleDto?>> GetArticle(int id) {
    var result = await newsRepository.GetArticleAsync(id);
    return Ok(result);
  }
  
  [HttpGet("/v1/articles")]
  public async Task<ActionResult<IEnumerable<NewsArticleDto>>> GetArticles() {
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
      Tags = article.Tags,
      PublicationDate = DateTime.UtcNow,
    };
    
    var result = await newsRepository.CreateArticleAsync(newsArticleCreateDto);
    return Ok(result);
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
    return Ok(result);
  }
  
  [HttpDelete("/v1/article/{id}")]
  public async Task<ActionResult> DeleteArticle(
    int id
  ) {
    await newsRepository.DeleteArticleAsync(id);
    return Ok();
  }
  
  
}