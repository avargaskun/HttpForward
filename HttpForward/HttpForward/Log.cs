using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpForward
{
    public static class Log
    {
        public static EventLog EventLog { get; set; }

        public static void Error(string message)
        {
            EventLog.WriteEntry(message, EventLogEntryType.Error);
        }

        public static void Warning(string message)
        {
            EventLog.WriteEntry(message, EventLogEntryType.Warning);
        }

        public static void Info(string message)
        {
            EventLog.WriteEntry(message, EventLogEntryType.Information);
        }
    }
}
