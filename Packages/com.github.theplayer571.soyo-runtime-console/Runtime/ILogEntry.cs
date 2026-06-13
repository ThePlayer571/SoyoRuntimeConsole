using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    public readonly struct LogEntry
    {
        public LogEntry(string condition, string stackTrace, LogType logType)
        {
            Condition = condition;
            StackTrace = stackTrace;
            LogType = logType;
        }

        public LogType LogType { get; }
        public string Condition { get; }
        public string StackTrace { get; }
    }
}