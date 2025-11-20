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
  [HttpGet("article/{id}")]
  public async Task<ActionResult<NewsArticleDto>> GetArticle(int id, CancellationToken cancellationToken = default) {
    var result = await newsRepository.GetArticleAsync(id, cancellationToken);
    if (result == null) return NotFound("статья не найдена");
    return OkResult(result);
  }
  
  [HttpGet("articles")]
  public async Task<ActionResult<IEnumerable<NewsArticleDto>>> GetArticles(CancellationToken cancellationToken = default) {
    var result = await newsRepository.GetArticlesAsync(cancellationToken);
    return OkResult(result);
  }
  
  [Authorize]
  [HttpPost("article")]
  public async Task<ActionResult<NewsArticleDto>> CreateArticle(
    [FromBody] ArticleCreateRequest article,
    CancellationToken cancellationToken = default
    ) {
    var userInfo = GetUserInfo();
    if (userInfo == null) return BadRequest("не удалось получить данные о пользователе");
    
    var result = await newsRepository.CreateArticleAsync(new NewsArticleCreateDto {
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      UserId = userInfo.Id,
      UserName = userInfo.Name
    }, cancellationToken);
    
    return OkResult(result);
  }
  
  [Authorize]
  [HttpPatch("article")]
  public async Task<ActionResult<NewsArticleDto>> UpdateArticle(
    [FromBody] ArticleUpdateRequest article,
    CancellationToken cancellationToken = default
  ) {
    var userInfo = GetUserInfo();
    if (userInfo == null) return BadRequest("не удалось получить данные о пользователе");
    
    var result = await newsRepository.UpdateArticleAsync(new NewsArticleUpdateDto {
      Id = article.Id,
      Title = article.Title,
      Content = article.Content,
      Summary = article.Summary,
      Tags = article.Tags,
      UserId = userInfo.Id, // теперь это его статья
      UserName = userInfo.Name
    }, cancellationToken);
    
    if (result == null) return NotFound("статья не найдена");
    
    return OkResult(result);
  }
  
  [Authorize]
  [HttpDelete("article/{id}")]
  public async Task<ActionResult> DeleteArticle(
    int id,
    CancellationToken cancellationToken = default
  ) {
    var success = await newsRepository.DeleteArticleAsync(id, cancellationToken);
    if (success == false) return NotFound("статья не найдена");
    
    return Ok();
  }
}