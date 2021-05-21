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

        /// <summary>
        /// Gets or sets the sub directory
        /// that the log files should be written to
        /// if the sub directory does not exist
        /// it will be created
        /// Default to Logs
        /// </summary>
        /// <value>
        /// The sub directory.
        /// </value>
        public static string SubDirectory { get; set; } = "Logs";

        /// <summary>
        /// Gets or sets the maximum time to keep.
        /// Defaults to 7 days
        /// </summary>
        /// <value>
        /// The maximum time to keep.
        /// </value>
        public static TimeSpan MaxTimeToKeep { get; set; } = new TimeSpan(7);

        /// <summary>
        /// Gets or sets the file extension to use for log files
        /// Defaults to .txt
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public static string FileExtension { get; set; } = ".txt";

        public static string Filename { get; set; }
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


            OnEntry?.Invoke(l, fmsg);

            if (string.IsNullOrEmpty(Filename))
            {
                return;
            }

            string fname = GetCurrentFileName();

            if (fileStream == null || lastFileName == null || !lastFileName.Equals(fname))
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                if (!string.IsNullOrEmpty(SubDirectory))
                {
                    //make sure directory exists
                    string sub = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SubDirectory);
                    if (!Directory.Exists(sub))
                    {
                        Directory.CreateDirectory(sub);
                    }
                }

                //check to remove old files
                RemoveOldFiles();

                fileStream = new StreamWriter(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fname), FileMode.Append, FileAccess.Write, FileShare.Read));
                lastFileName = fname;
            }

            if (fileStream != null)
            {
                fileStream.WriteLine(fmsg);
                fileStream.Flush();
            }
        }

        protected static void RemoveOldFiles()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string actualPath = null;
            if (!string.IsNullOrEmpty(SubDirectory))
            {
                actualPath = Path.Combine(basePath, SubDirectory);
            }
            else
            {
                actualPath = basePath;
            }

            if (!Directory.Exists(actualPath)) return;

            string[] files = Directory.GetFiles(actualPath, "*" + FileExtension);
            for (int i = 0; i < files.Length; ++i)
            {
                string absolutePath = Path.Combine(actualPath, files[i]);
                if (!File.Exists(absolutePath)) continue;
                DateTime t = File.GetCreationTime(absolutePath);
                long currentTime = DateTime.Now.Ticks;
                TimeSpan span = new TimeSpan(currentTime - t.Ticks);
                if (span >= MaxTimeToKeep)
                {
                    File.Delete(absolutePath);
                }
            }
        }

        protected static string GetCurrentFileName()
        {
            if (string.IsNullOrEmpty(SubDirectory)) return Filename + FileExtension;
            return Path.Combine(SubDirectory, Filename + FileExtension);

            //DateTime d = DateTime.Now;
            //return d.ToString(File);
        }
    }
}
