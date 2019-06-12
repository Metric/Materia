using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Materia.Logging
{
    public enum MateriaLogLevel
    {
        All = 0,
        Debug = 2,
        Trace = 4,
        Error = 8,
        Warn = 16,
        Info = 32,
        Fatal = 64
    }

    [Target("mlog")]
    public class MateriaLogTarget : TargetWithLayout
    {
        //defaults to 1MB of constant maintained log size
        public delegate void LogWrite(MateriaLogLevel level, string msg);
        public static event LogWrite OnLogWrite;

        public MateriaLogTarget()
        {
            
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string msg = Layout.Render(logEvent).Replace("|", " ");

            if(OnLogWrite != null)
            {
                MateriaLogLevel level = MateriaLogLevel.All;

                if (logEvent.Level.Ordinal == 0)
                {
                    level = MateriaLogLevel.Trace; 
                }
                else if(logEvent.Level.Ordinal == 1)
                {
                    level = MateriaLogLevel.Debug;
                }
                else if(logEvent.Level.Ordinal == 2)
                {
                    level = MateriaLogLevel.Info;
                }
                else if(logEvent.Level.Ordinal == 3)
                {
                    level = MateriaLogLevel.Warn;
                }
                else if(logEvent.Level.Ordinal == 4)
                {
                    level = MateriaLogLevel.Error;
                }
                else if(logEvent.Level.Ordinal == 5)
                {
                    level = MateriaLogLevel.Fatal;
                }
   
                if(OnLogWrite != null)
                {
                    OnLogWrite.Invoke(level, msg);
                }
            }
        }
    }
}
