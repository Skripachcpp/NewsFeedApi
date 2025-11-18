using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController(ITagsRepository tagsRepository) : ControllerBase {
  [HttpGet("v1/tags")]
  public async Task<ActionResult<IEnumerable<TagDto>>> GetTags(CancellationToken cancellationToken = default) {
    var result = await tagsRepository.GetTags(cancellationToken);
    return Ok(result);
  }

  [HttpDelete("v1/tags/{id}")]
  public async Task<ActionResult> DeleteTag(int id, CancellationToken cancellationToken = default) {
    await tagsRepository.DeleteTag(id, cancellationToken);
    return Ok();
  }
}