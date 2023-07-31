using LiteDB;
using PunchBotCore2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchBotCore2.Util
{
    public static class TimeUtils
    {
        public static List<Activity> GetDailyTimeSpans(this LiteDatabase db, DateTime time)
        {
            var startOfDay = time.Date;
            return db.GetWorkTimeSpansForQuery(Query.And(Query.GTE("Time", startOfDay), Query.LTE("Time", time)));
        }

        public static List<Activity> GetDailyBreakTimeSpans(this LiteDatabase db, DateTime time)
        {
            var startOfDay = time.Date;
            return db.GetBreakTimeSpansForQuery(Query.And(Query.GTE("Time", startOfDay), Query.LTE("Time", time)));
        }

        public static List<Activity> GetWeeklyTimeSpans(this LiteDatabase db, DateTime time)
        {
            var differenceToMonday = ((int)time.DayOfWeek + 6) % 7;
            var monday = time.AddDays(-differenceToMonday).Date;
            return db.GetWorkTimeSpansForQuery(Query.And(Query.GTE("Time", monday), Query.LTE("Time", time)));
        }

        public static List<Activity> GetMonthlyTimeSpans(this LiteDatabase db, DateTime time)
        {
            var firstOfMonth = new DateTime(time.Year, time.Month, 1);
            return db.GetWorkTimeSpansForQuery(Query.And(Query.GTE("Time", firstOfMonth), Query.LTE("Time", time)));
        }

        public static List<Activity> GetAllTimeSpans(this LiteDatabase db, DateTime until)
        {
            return db.GetWorkTimeSpansForQuery(Query.LTE("Time", until));
        }

        private static List<Activity> GetWorkTimeSpansForQuery(this LiteDatabase db, BsonExpression query)
        {
            var col = db.GetCollection<PunchEntry>(PunchEntry.TableName);
            col.EnsureIndex(x => x.Time);
            var entries = col.Find(query);
            DateTime? lastPunchInTime = null;
            var timeSpans = new List<Activity>();
            foreach (var punch in entries)
            {
                switch (punch.Kind)
                {
                    case Kind.In:
                        lastPunchInTime = punch.Time;
                        break;
                    case Kind.Out:
                        if (lastPunchInTime == null) { continue; }
                        timeSpans.Add(new Activity { Start = lastPunchInTime.Value, End = punch.Time });
                        lastPunchInTime = null;
                        break;
                }
            }
            if (lastPunchInTime != null)
            {
                timeSpans.Add(new Activity { Start = lastPunchInTime.Value });
            }
            return timeSpans;
        }

        private static List<Activity> GetBreakTimeSpansForQuery(this LiteDatabase db, BsonExpression query)
        {
            var col = db.GetCollection<PunchEntry>(PunchEntry.TableName);
            col.EnsureIndex(x => x.Time);
            var entries = col.Find(query).Skip(1);
            DateTime? lastPunchOutTime = null;
            var timeSpans = new List<Activity>();
            foreach (var punch in entries)
            {
                switch (punch.Kind)
                {
                    case Kind.Out:
                        lastPunchOutTime = punch.Time;
                        break;
                    case Kind.In:
                        if (lastPunchOutTime == null) { continue; }
                        timeSpans.Add(new Activity { Start = lastPunchOutTime.Value, End = punch.Time });
                        lastPunchOutTime = null;
                        break;
                }
            }
            if (lastPunchOutTime != null)
            {
                timeSpans.Add(new Activity { Start = lastPunchOutTime.Value });
            }
            return timeSpans;
        }
    }
}