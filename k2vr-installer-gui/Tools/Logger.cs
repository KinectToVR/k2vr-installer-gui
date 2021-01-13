using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k2vr_installer_gui.Tools
{
    public class LogEventArgs : EventArgs
    {
        public string text;
        public bool isUserRelevant;

        public LogEventArgs(string text, bool isUserRelevant)
        {
            this.text = text;
            this.isUserRelevant = isUserRelevant;
        }
    }

    public delegate void LogEventHandler(LogEventArgs e);

    public static class Logger
    {
        public static event LogEventHandler LogEvent;

        public static void Log(string text, bool newLine = true, bool isUserRelevant = true)
        {
            if (newLine) text += Environment.NewLine;
            File.AppendAllText(Path.Combine(App.downloadDirectory, "install.log"), text + Environment.NewLine);
            LogEvent?.Invoke(new LogEventArgs(text, isUserRelevant));
        }
    }
}
