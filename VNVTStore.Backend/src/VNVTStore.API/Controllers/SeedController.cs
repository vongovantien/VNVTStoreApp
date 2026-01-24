using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Seeding;

namespace VNVTStore.API.Controllers;

[ApiController]
[Route("api/v1/seed")]
public class SeedController : ControllerBase
{
    private readonly DataSeeder _dataSeeder;

    public SeedController(DataSeeder dataSeeder)
    {
        _dataSeeder = dataSeeder;
    }

    [HttpPost]
    public async Task<IActionResult> Seed()
    {
        await _dataSeeder.SeedAsync();
        return Ok(new { message = "Seeding completed successfully (200 Users, 20 Categories, 200 Products, 200 Orders)." });
    }
}
