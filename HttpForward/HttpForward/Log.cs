using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpForward
{
    public static class Log
    {
        public static TextWriter Out { get; set; } = new StreamWriter(File.Open(GetLogFilename(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read));

        public static void Debug(string message)
        {
            Out.WriteLine($"[{DateTime.Now:G}] {message}");
            Out.Flush();
        }

        public static string GetLogFilename()
        {
            string my = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (my != null && Directory.Exists(my))
            {
                return Path.Combine(my, "HttpForward.log");
            }

            return Path.GetTempFileName();
        }
    }
}
