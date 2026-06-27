using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Models;
using LiteDB;
using PunchBotCore2.Util;

namespace PunchBotCore2.Controllers
{
    public class JsonController : Controller
    {
        private readonly LiteDatabase _db;

        public JsonController(LiteDatabase db)
        {
            _db = db;
        }

        public JsonResult Week()
        {
            var week = new Week { TimeSpans = _db.GetWeeklyTimeSpans(DateTime.Now) };
            return new JsonResult(week);
        }

        public JsonResult ListAll()
        {
            var result = _db.GetCollection<PunchEntry>(PunchEntry.TableName).FindAll().OrderByDescending(x => x.Time).ToList();
            return new JsonResult(result);
        }

        public JsonResult Get(int id)
        {
            return new JsonResult(_db.GetCollection<PunchEntry>(PunchEntry.TableName).FindById(id));
        }

        [HttpPatch]
        public ActionResult Patch(PunchEntry entry)
        {
            _db.GetCollection<PunchEntry>(PunchEntry.TableName).Update(entry);
            return Ok();
        }

        public ActionResult Delete(int id)
        {
            _db.GetCollection<PunchEntry>(PunchEntry.TableName).Delete(id);
            return NoContent();
        }
    }
}