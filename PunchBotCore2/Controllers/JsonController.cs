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
        Week week = new() { TimeSpans = context.GetWeeklyTimeSpans(dateTimeService.Now) };
        return new JsonResult(week);
    }

    public async Task<JsonResult> ListAll()
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        List<PunchEntry> result = context.PunchEntries
            .OrderByDescending(x => x.Time)
            .ToList();

        return new JsonResult(result);
    }

    public async Task<JsonResult> Get(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        return new JsonResult(context.PunchEntries.Find(id));
    }

    [HttpPatch]
    public async Task<ActionResult> Patch(PunchEntry entry)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries
            .Where(e => e.Id == entry.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Time, _ => entry.Time));
        await context.SaveChangesAsync();
        return Ok();
    }

    public async Task<ActionResult> Delete(int id)
    {
        using PunchContext context = await contextFactory.CreateDbContextAsync();
        await context.PunchEntries.Where(e => e.Id == id).ExecuteDeleteAsync();

        return NoContent();
    }
}