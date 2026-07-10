using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using PunchBotCore2.Data;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers;

public class JsonController(
    IDbContextFactory<PunchContext> contextFactory,
    IDataAggregatorFactory aggregatorFactory,
    IDateTimeService dateTimeService) : Controller
{
    public async Task<JsonResult> Overview()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DataAggregator aggregator = aggregatorFactory.Create(context);
        return new JsonResult(await aggregator.GetIndexData());
    }

    public async Task<JsonResult> Week()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        DataAggregator aggregator = aggregatorFactory.Create(context);
        Week week = new() { TimeSpans = await aggregator.GetWeeklyTimeSpans(dateTimeService.Now) };
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