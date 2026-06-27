using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers
{
    public class HomeController(LiteDatabase db) : Controller
    {
        private readonly TimeSpan DailyWorkTime = TimeSpan.FromHours(7);
        private readonly LiteDatabase _db = db;
        private readonly TimeSpan minBreakDuration = TimeSpan.FromMinutes(30);

        public ActionResult Index()
        {
            return View(GetIndexData());
        }

        private IndexData GetIndexData()
        {
            ILiteCollection<PunchEntry> col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);
            PunchEntry lastEntry = col.FindOne(Query.All(Query.Descending));
            DateTime now = DateTime.Now;
            TimeSpan totalSum = _db.GetAllTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);

            var numDays = col.FindAll().GroupBy(x => x.Time.Date).Count();
            TimeSpan remainingTime = numDays * DailyWorkTime - totalSum;
            if (remainingTime <= TimeSpan.Zero)
            {
                remainingTime = DailyWorkTime + remainingTime;
            }
            TimeSpan daySum = _db.GetDailyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            TimeSpan dayBreakSum = _db.GetDailyBreakTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
            DateTime estimatedEnd = dayBreakSum >= minBreakDuration ? DateTime.Now + remainingTime : DateTime.Now + remainingTime + minBreakDuration - dayBreakSum;
            IndexData indexData = new(
                weekSum: _db.GetWeeklyTimeSpans(now).Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration),
                daySum: daySum,
                lastEntry: lastEntry,
                remainingTime: remainingTime,
                dayBreakSum: dayBreakSum,
                estimatedEnd: estimatedEnd
            );

            return indexData;
        }

        [HttpPost]
        public ActionResult Punch()
        {
            DateTime now = DateTime.Now;

            ILiteCollection<PunchEntry> col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);

            PunchEntry lastEntry = col.FindOne(Query.All(Query.Descending));
            Kind lastKind = lastEntry?.Kind ?? Kind.Out;

            col.Insert(new PunchEntry { Kind = lastKind == Kind.In ? Kind.Out : Kind.In, Time = now });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Holiday()
        {
            DateTime today = DateTime.Today;
            DateTime startTime = today.AddHours(8);
            DateTime endTime = startTime + DailyWorkTime;

            ILiteCollection<PunchEntry> col = _db.GetCollection<PunchEntry>(PunchEntry.TableName);

            col.Insert(new PunchEntry { Kind = Kind.In, Time = startTime });
            col.Insert(new PunchEntry { Kind = Kind.Out, Time = endTime });
            return RedirectToAction("Index");
        }

        public ActionResult Week()
        {
            Week week = new() { TimeSpans = _db.GetWeeklyTimeSpans(DateTime.Now) };
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

        public ContentResult Export()
        {
            const string header = "insert into\n    punch_entries(id, time, kind)\nvalues\n";
            IEnumerable<string> result = _db.GetCollection<PunchEntry>(PunchEntry.TableName)
                .FindAll()
                .OrderBy(x => x.Time)
                .Select(x => x.ToSqlRow());

            return new ContentResult
            {
                Content = header + string.Join(",\n", result) + ";",
                ContentType = "application/sql",
                StatusCode = 200
            };
        }

        public IActionResult Error()
        {
            return base.View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
