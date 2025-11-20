using System.Security.Claims;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
    if (result == null) return NotFound("Статья не найдена");
    return OkResult(result);
  }
  
  [HttpGet("/v1/articles")]
  public async Task<ActionResult<IEnumerable<NewsArticleDto>>> GetArticles() {
    var result = await newsRepository.GetArticlesAsync();
    return OkResult(result);
  }
  
  [Authorize]
  [HttpPost("/v1/article")]
  public async Task<ActionResult<NewsArticleDto>> CreateArticle(
    [FromBody] ArticleCreateRequest article
    ) {
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var userName = User.FindFirst(ClaimTypes.Name)!.Value;
    
    var result = await newsRepository.CreateArticleAsync(new NewsArticleCreateDto {
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      UserId = userId,
      UserName = userName
    });
    
    return OkResult(result);
  }
  
  [Authorize]
  [HttpPatch("/v1/article")]
  public async Task<ActionResult<NewsArticleDto>> UpdateArticle(
    [FromBody] ArticleUpdateRequest article
  ) {
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var userName = User.FindFirst(ClaimTypes.Name)!.Value;
    
    var result = await newsRepository.UpdateArticleAsync(new NewsArticleUpdateDto {
      Id = article.Id,
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      UserName = userName,
      UserId = userId // теперь это его статья
    });
    
    if (result == null) return NotFound("Статья не найдена");
    
    return OkResult(result);
  }
  
  [Authorize]
  [HttpDelete("/v1/article/{id}")]
  public async Task<ActionResult> DeleteArticle(
    int id
  ) {
    var success = await newsRepository.DeleteArticleAsync(id);
    if (success == false) return NotFound("Статья не найдена");
    
    return Ok();
  }
}