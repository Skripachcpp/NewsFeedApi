using Domain.DTOs;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Application;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TagsController(ICacheRepository cacheRepository, ITagsRepository tagsRepository): BaseController
{
    [HttpGet("tags")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags(CancellationToken cancellationToken = default)
    {
        var result = await tagsRepository.GetTags(cancellationToken).ConfigureAwait(false);
        return this.OkResult(result);
    }

    [Authorize]
    [HttpDelete("tags/{id}")]
    public async Task<ActionResult> DeleteTag(int id, CancellationToken cancellationToken = default)
    {
        await tagsRepository.DeleteTag(id, cancellationToken).ConfigureAwait(false);
        
        await cacheRepository.Clear(CacheKeys.Articles);
        return this.Ok();
    }
}