using Claims.Domain;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly CoverService _covers;

    public CoversController(CoverService covers)
    {
        _covers = covers;
    }

    [HttpPost("compute")]
    public ActionResult ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(_covers.ComputePremium(startDate, endDate, coverType));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        return Ok(await _covers.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        return Ok(await _covers.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        return Ok(await _covers.CreateAsync(cover));
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _covers.DeleteAsync(id);
    }
}
