using Claims.Contracts;
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
    public async Task<ActionResult> CreateAsync(CreateClaimRequest request)
    {
        return this.ToActionResult(await _claims.CreateAsync(request));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        return this.ToActionResult(await _claims.DeleteAsync(id));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Claim>> GetAsync(string id)
    {
        var claim = await _claims.GetByIdAsync(id);
        return claim is null ? NotFound() : Ok(claim);
    }
}
