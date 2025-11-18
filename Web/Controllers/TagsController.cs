using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController(ITagsRepository tagsRepository) : ControllerBase {
  // [HttpGet("v1/tags")]
  // public async Task<Tag> GetTags(CancellationToken cancellationToken = default) {
  //   tagsRepository.GetTags(cancellationToken)
  // }
  
  // [HttpPost("/v1/test")]
  // public async Task<ActionResult> Test(
  //   CancellationToken cancellationToken = default) {
  //   await tagsRepository.CreateIfNotExistsAsync(new List<string> {"Спорт", "Популярное"}, cancellationToken);
  //   return Ok();
  // }
}