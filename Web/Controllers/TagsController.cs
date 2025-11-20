using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Application;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController(ITagsRepository tagsRepository) : BaseController {
  [HttpGet("tags")]
  public async Task<ActionResult<IEnumerable<TagDto>>> GetTags(CancellationToken cancellationToken = default) {
    var result = await tagsRepository.GetTags(cancellationToken);
    return OkResult(result);
  }

  [Authorize]
  [HttpDelete("tags/{id}")]
  public async Task<ActionResult> DeleteTag(int id, CancellationToken cancellationToken = default) {
    await tagsRepository.DeleteTag(id, cancellationToken);
    return Ok();
  }
}