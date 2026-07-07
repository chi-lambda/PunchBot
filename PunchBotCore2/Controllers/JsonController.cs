using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Data;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers;

public class JsonController(IDbContextFactory<PunchContext> contextFactory, IDateTimeService dateTimeService) : Controller
{
    public async Task<JsonResult> Week()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        Week week = new() { TimeSpans = await context.GetWeeklyTimeSpans(dateTimeService.Now) };
        return new JsonResult(week);
    }

    public async Task<JsonResult> ListAll()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        List<PunchEntry> result = await context.PunchEntries
            .OrderByDescending(x => x.Time)
            .ToListAsync();

        return new JsonResult(result);
    }

    public async Task<IActionResult> Get(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        PunchEntry? entry = context.PunchEntries.Find(id);
        return entry is not null ? new JsonResult(entry) : NotFound();
    }

    [HttpPatch]
    public async Task<ActionResult> Patch(PunchEntry entry)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        int updatedRows = await context.PunchEntries
            .Where(e => e.Id == entry.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Time, _ => entry.Time));
        return updatedRows > 0 ? Ok() : NotFound();
    }

    public async Task<ActionResult> Delete(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries.Where(e => e.Id == id).ExecuteDeleteAsync();

        return NoContent();
    }
}