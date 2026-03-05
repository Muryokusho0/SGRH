using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGRH.Persistence.Context;

namespace SGRH.Api.Controllers;

[ApiController]
[Route("api/test-db")]
public class TestDbController : ControllerBase
{
    private readonly SGRHDbContext _db;

    public TestDbController(SGRHDbContext db)
    {
        _db = db;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var canConnect = await _db.Database.CanConnectAsync();
        return Ok(new { Connected = canConnect });
    }
}