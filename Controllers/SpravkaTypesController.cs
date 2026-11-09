using ClinicDataApi.Contracts;
using ClinicDataApi.Data;
using ClinicDataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SpravkaTypesController(ClinicDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpravkaTypeResponse>>> GetAll([FromQuery] bool priceDescending = false)
    {
        var query = db.SpravkaTypes.AsNoTracking();
        query = priceDescending ? query.OrderByDescending(type => type.Price) : query.OrderBy(type => type.Price);
        var items = await query.Select(type => new SpravkaTypeResponse(type.Id, type.Name, type.Price, type.ValidMonths, type.Description))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpravkaTypeResponse>> GetById(int id)
    {
        var item = await db.SpravkaTypes.AsNoTracking().Where(type => type.Id == id)
            .Select(type => new SpravkaTypeResponse(type.Id, type.Name, type.Price, type.ValidMonths, type.Description))
            .FirstOrDefaultAsync();
        return item is null ? NotFound(new { message = $"Вид справки с id={id} не найден." }) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<SpravkaTypeResponse>> Create(SpravkaTypeRequest request)
    {
        var type = new SpravkaType
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            ValidMonths = request.ValidMonths,
            Description = request.Description.Trim()
        };
        db.SpravkaTypes.Add(type);
        await db.SaveChangesAsync();
        var response = ToResponse(type);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpravkaTypeResponse>> Update(int id, SpravkaTypeRequest request)
    {
        var type = await db.SpravkaTypes.FindAsync(id);
        if (type is null)
        {
            return NotFound(new { message = $"Вид справки с id={id} не найден." });
        }

        type.Name = request.Name.Trim();
        type.Price = request.Price;
        type.ValidMonths = request.ValidMonths;
        type.Description = request.Description.Trim();
        await db.SaveChangesAsync();
        return Ok(ToResponse(type));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await db.SpravkaTypes.FindAsync(id);
        if (type is null)
        {
            return NotFound(new { message = $"Вид справки с id={id} не найден." });
        }
        if (await db.Spravki.AnyAsync(spravka => spravka.SpravkaTypeId == id))
        {
            return Conflict(new { message = "Нельзя удалить вид справки, который уже использован в выданных справках." });
        }

        db.SpravkaTypes.Remove(type);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static SpravkaTypeResponse ToResponse(SpravkaType type) =>
        new(type.Id, type.Name, type.Price, type.ValidMonths, type.Description);
}
