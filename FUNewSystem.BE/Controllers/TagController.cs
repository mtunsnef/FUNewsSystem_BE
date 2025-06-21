using FUNewsSystem.Domain.Models;
using FUNewsSystem.Service.DTOs.ResponseDto;
using FUNewsSystem.Service.DTOs.TagDto;
using FUNewsSystem.Service.Services.TagService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FUNewSystem.BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: api/tag
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll()
        {
            var tags = await _tagService.GetAllTag();
            return Ok(tags);
        }

        // GET: api/tag/{id}
        [HttpGet("{id}")]
        [EnableQuery]
        public async Task<ActionResult<Tag>> GetById(Guid id)
        {
            var tag = await _tagService.GetByTagId(id);
            return Ok(tag);
        }

        // POST: api/tag
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUpdateTagDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _tagService.CreateTag(dto);
            return Ok(response);
        }

        // PUT: api/tag/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateTagDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _tagService.UpdateTag(id, dto);
            return Ok(response);
        }

        // DELETE: api/tag/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _tagService.DeleteTag(id);
            return Ok(response);
        }
    }
}
