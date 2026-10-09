using Claims.Domain;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly ClaimService _claims;

    public ClaimsController(ClaimService claims)
    {
        _claims = claims;
    }

    [HttpGet]
    public async Task<IEnumerable<Claim>> GetAsync()
    {
        return await _claims.GetAllAsync();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(Claim claim)
    {
        return Ok(await _claims.CreateAsync(claim));
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _claims.DeleteAsync(id);
    }

    [HttpGet("{id}")]
    public async Task<Claim?> GetAsync(string id)
    {
        return await _claims.GetByIdAsync(id);
    }
}
