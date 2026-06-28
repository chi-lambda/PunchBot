using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Data;

namespace PunchBotCore2.Controllers;

public class JsonController(PunchContext context) : Controller
{
    public JsonResult Week()
    {
        Week week = new() { TimeSpans = context.GetWeeklyTimeSpans(DateTime.Now) };
        return new JsonResult(week);
    }

    public JsonResult ListAll()
    {
        List<PunchEntry> result = context.PunchEntries
            .OrderByDescending(x => x.Time)
            .ToList();

        return new JsonResult(result);
    }

    public JsonResult Get(int id)
    {
        return new JsonResult(context.PunchEntries.Find(id));
    }

    [HttpPatch]
    public async Task<ActionResult> Patch(PunchEntry entry)
    {
        context.PunchEntries.Update(entry);
        await context.SaveChangesAsync();
        return Ok();
    }

    public async Task<ActionResult> Delete(int id)
    {
        PunchEntry? entryToDelete = context.PunchEntries.Find(id);
        if (entryToDelete is not null)
        {
            context.PunchEntries.Remove(entryToDelete);
            await context.SaveChangesAsync();
        }

        return NoContent();
    }
}