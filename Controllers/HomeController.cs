using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Util;

namespace PunchBotCore.Controllers
{
    public class HomeController : Controller
    {
        private const string dbFilename = "times.db";
        private readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);

        public ActionResult Index()
        {
            return View(GetIndexData());
        }

        private IndexData GetIndexData()
        {
            using var db = new LiteDatabase(dbFilename);
            var col = db.GetCollection<PunchEntry>(PunchEntry.TableName);
            var lastEntry = col.FindOne(Query.All(Query.Descending));
            var now = DateTime.Now;
            var totalSum = db.GetAllTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var numDays = col.FindAll().GroupBy(x => x.Time.Date).Count();
            var remainingTime = numDays * DailyWorkTime - totalSum;
            var daySum = db.GetDailyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var dayBreakSum = db.GetDailyBreakTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var indexData = new IndexData(
                weekSum: db.GetWeeklyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
                daySum: daySum,
                lastEntry: lastEntry,
                remainingTime: remainingTime,
                dayBreakSum: dayBreakSum
            );

            return indexData;
        }

        [HttpPost]
        public ActionResult Punch()
        {
            using var db = new LiteDatabase(dbFilename);
            var now = DateTime.Now;

            var col = db.GetCollection<PunchEntry>(PunchEntry.TableName);

            var lastEntry = col.FindOne(Query.All(Query.Descending));
            var lastTime = lastEntry?.Time ?? now;
            var lastKind = lastEntry?.Kind ?? Kind.Out;

            var message = lastKind == Kind.In ? $"Punched out after {now - lastTime}" : $"Punched in";

            col.Insert(new PunchEntry { Kind = lastKind == Kind.In ? Kind.Out : Kind.In, Time = now });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Holiday()
        {
            using var db = new LiteDatabase(dbFilename);
            var today = DateTime.Today;
            var startTime = today.AddHours(8);
            var endTime = startTime + DailyWorkTime;

            var col = db.GetCollection<PunchEntry>(PunchEntry.TableName);

            col.Insert(new PunchEntry { Kind = Kind.In, Time = startTime });
            col.Insert(new PunchEntry { Kind = Kind.Out, Time = endTime });
            return RedirectToAction("Index");
        }

        public ActionResult Week()
        {
            using var db = new LiteDatabase(dbFilename);
            var week = new Week { TimeSpans = db.GetWeeklyTimeSpans(DateTime.Now) };
            return View(week);
        }

        // public ActionResult Clear()
        // {
        //     using var db = new LiteDatabase(dbFilename);
        //     db.DropCollection(PunchEntry.TableName);
        //     return Redirect("Index");
        // }

        public ActionResult ListAll()
        {
            using var db = new LiteDatabase(dbFilename);
            return View(db.GetCollection<PunchEntry>(PunchEntry.TableName).FindAll().OrderByDescending(x => x.Time).ToList());
        }

        public ActionResult Edit(int id)
        {
            using var db = new LiteDatabase(dbFilename);
            return View(db.GetCollection<PunchEntry>(PunchEntry.TableName).FindById(id));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(PunchEntry entry)
        {
            using var db = new LiteDatabase(dbFilename);
            db.GetCollection<PunchEntry>(PunchEntry.TableName).Update(entry);
            return RedirectToAction("ListAll");
        }

        public ActionResult Delete(int id)
        {
            using var db = new LiteDatabase(dbFilename);
            var deleted = db.GetCollection<PunchEntry>(PunchEntry.TableName).Delete(id);
            return RedirectToAction("ListAll");
        }

        // public FileResult DownloadDatabase()
        // {
        //     var content = System.IO.File.ReadAllBytes(dbFilename);
        //     return File(content, "application/octet-stream", "times.db");
        // }

        public IActionResult Error()
        {
            return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
