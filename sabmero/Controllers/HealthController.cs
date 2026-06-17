using Microsoft.AspNetCore.Mvc;
using sabmero.Data;

namespace sabmero.Controllers;

// ── GET /api/health ───────────────────────────────────────────────────────────
// Flutter developer calls this first to confirm server + database are alive.
// No login required.

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Check()
    {
        bool dbOk;
        try { dbOk = _db.Database.CanConnect(); }
        catch { dbOk = false; }

        return Ok(new
        {
            status = "sabmero API is running ✅",
            database = dbOk ? "PostgreSQL connected ✅" : "Database NOT connected ❌",
            timestamp = DateTime.UtcNow,
            phase = "All phases (1–5) — Auth, Shop, Services, Admin, Payments"
        });
    }
}