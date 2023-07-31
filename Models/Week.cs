using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchBotCore2.Models
{
    public class Week
    {
        public List<Activity> TimeSpans { get; set; } = new List<Activity>();
        public TimeSpan Sum => TimeSpans.Aggregate(TimeSpan.Zero, (acc, x) => acc + x.Duration);
    }
}