using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MLog
{
    public enum LogLevel
    {
        All = 0,
        Debug = 2,
        Trace = 4,
        Error = 8,
        Warn = 16,
        Info = 32,
        Fatal = 64
    }

    public class Log
    {
        protected static string lastFileName;
        protected static StreamWriter fileStream;

        public static event Action<LogLevel, string> OnEntry;

        public static string File { get; set; }
        public static Func<LogLevel, string, string, string> Formatter { get; set; }

        public static LogLevel Levels { get; set; }

        public static void Any(LogLevel l, string msg, [CallerMemberName] string caller = "")
        {
            Write(l, caller, msg);
        }

        public static void Any(LogLevel l, Exception e, [CallerMemberName] string caller = "")
        {
            Write(l, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Fatal(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Fatal, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Fatal(string msg, [CallerMemberName] string caller = "")
        {

            Write(LogLevel.Fatal, caller, msg);
        }

        public static void Info(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Info, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Info(string msg, [CallerMemberName] string caller = "")
        {

            Write(LogLevel.Info, caller, msg);
        }

        public static void Error(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Error, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Error(string msg, [CallerMemberName] string caller = "")
        {

            Write(LogLevel.Error, caller, msg);
        }

        public static void Trace(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Trace, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Trace(string msg, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Trace, caller, msg);
        }

        public static void Debug(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Debug, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Debug(string msg, [CallerMemberName] string caller = "")
        {

            Write(LogLevel.Debug, caller, msg);
        }

        public static void Warn(Exception e, [CallerMemberName] string caller = "")
        {
            Write(LogLevel.Warn, caller, e.Message + " - " + e.StackTrace);
        }

        public static void Warn(string msg, [CallerMemberName] string caller = "")
        {

            Write(LogLevel.Warn, caller, msg);
        }

        protected static string DefaultFormatter(LogLevel l, string caller, string msg)
        {
            return $"({l.ToString().ToUpper()})({(string.IsNullOrEmpty(caller) ? "Unknown" : caller)}): {msg}";
        }

        protected static void Write(LogLevel l, string caller, string msg)
        {

            string fmsg = Formatter?.Invoke(l, caller, msg);
            if (string.IsNullOrEmpty(fmsg))
            {
                fmsg = DefaultFormatter(l, caller, msg);
            }


            OnEntry(l, fmsg);

            if (string.IsNullOrEmpty(File))
            {
                return;
            }

            string fname = GetCurrentFileName();
            fname += ".txt";

            if (fileStream == null || lastFileName == null || !lastFileName.Equals(fname))
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                fileStream = new StreamWriter(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fname), FileMode.Append, FileAccess.Write, FileShare.Read));
                lastFileName = fname;
            }

            if (fileStream != null)
            {
                fileStream.WriteLine(fmsg);
            }
        }

        protected static string GetCurrentFileName()
        {
            DateTime d = DateTime.Now;
            return d.ToString(File);
        }
    }
}
