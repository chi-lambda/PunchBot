using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Util;

namespace PunchBotCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
        private readonly LiteDatabase _db;

        public HomeController(LiteDatabase db)
        {
            _db = db;
        }

        public ActionResult Index()
        {
            return View(GetIndexData());
        }

        private IndexData GetIndexData()
        {
            var col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);
            var lastEntry = col.FindOne(Query.All(Query.Descending));
            var now = DateTime.Now;
            var totalSum = _db.GetAllTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var numDays = col.FindAll().GroupBy(x => x.Time.Date).Count();
            var remainingTime = numDays * DailyWorkTime - totalSum;
            var daySum = _db.GetDailyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var dayBreakSum = _db.GetDailyBreakTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            var indexData = new IndexData(
                weekSum: _db.GetWeeklyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
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
            var now = DateTime.Now;

            var col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);

            var lastEntry = col.FindOne(Query.All(Query.Descending));
            var lastKind = lastEntry?.Kind ?? Kind.Out;

            col.Insert(new PunchEntry { Kind = lastKind == Kind.In ? Kind.Out : Kind.In, Time = now });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Holiday()
        {
            var today = DateTime.Today;
            var startTime = today.AddHours(8);
            var endTime = startTime + DailyWorkTime;

            var col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);

            col.Insert(new PunchEntry { Kind = Kind.In, Time = startTime });
            col.Insert(new PunchEntry { Kind = Kind.Out, Time = endTime });
            return RedirectToAction("Index");
        }

        public ActionResult Week()
        {
            var week = new Week { TimeSpans = _db.GetWeeklyTimeSpans(DateTime.Now) };
            return View(week);
        }

        // public ActionResult Clear()
        // {
        //     using var _db = new LiteDatabase(dbFilename);
        //     _db.DropCollection(PunchEntry.TableName);
        //     return Redirect("Index");
        // }

        public ActionResult ListAll()
        {
            return View(_db.GetCollection<PunchEntry>(PunchEntry.TableName).FindAll().OrderByDescending(x => x.Time).ToList());
        }

        public ActionResult Edit(int id)
        {
            return View(_db.GetCollection<PunchEntry>(PunchEntry.TableName).FindById(id));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(PunchEntry entry)
        {
            _db.GetCollection<PunchEntry>(PunchEntry.TableName).Update(entry);
            return RedirectToAction("ListAll");
        }

        public ActionResult Delete(int id)
        {
            _db.GetCollection<PunchEntry>(PunchEntry.TableName).Delete(id);
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
