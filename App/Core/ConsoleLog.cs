using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiClient.App.Models.AppContext;

namespace ApiClient.App.Core
{
    public enum LogType { Error, Sucess, Info }
    public class ConsoleLog
    {
        public ConsoleLog(string content, LogType type = LogType.Info)
        {
            var caller = new StackTrace().GetFrame(1)?.GetMethod();
            if (!(bool)Globals.Logging.Value) return;
            var record = new log()
            {
                Content = content,
                Rerturn = type,
                Space = caller.DeclaringType?.Namespace + "."
                      + caller.DeclaringType?.Name + "["
                      + caller.Name + "]"
                      ??
                      caller.ToString()
            };
            print(log: record);
        }
        public struct log
        {
            public string Content { get; set; }
            public string Space { get; set; }
            public LogType @Rerturn { get; set; }
        }
        public static void print(log log)
        {
            if (log.Rerturn == LogType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    "Error: on "
                    + log.Space + " as "
                    + log.Content
                    );
                Console.ResetColor();
                return;
            }
            if (log.Rerturn == LogType.Sucess)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(
                    "[Terminated at "
                    + log.Space + ": as "
                    + log.Content
                    + "]"
                    );
                Console.ResetColor();
                return;
            }
            Console.WriteLine(
                "at "
                + log.Space + ": as "
                + log.Content
                );
        }
    }
}
