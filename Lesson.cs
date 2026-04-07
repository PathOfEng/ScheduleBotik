using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleBotik
{
    public class Lesson
    {
        public string Name { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public Lesson(string name, TimeSpan beginTime, TimeSpan endTime)
        {
            Name = name;
            BeginTime = beginTime;
            EndTime = endTime;
        }
    }
}
