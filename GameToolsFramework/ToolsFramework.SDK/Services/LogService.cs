using ToolsFramework.SDK.Interfaces;

namespace ToolsFramework.SDK.Services
{
    // stores the logs in memory and notifies subscribers via the events
    public class LogService : ILogService
    {
        private readonly List<LogEntry> _logs = new();
        private readonly object _lock = new();

        public event Action<LogEntry>? OnLogEntry;

        public void Info(string source, string message) => Log("INFO", source, message);
        public void Warning(string source, string message) => Log("WARNING", source, message);
        public void Error(string source, string message) => Log("ERROR", source, message);

        public List<LogEntry> GetRecentLogs(int count = 100)
        {
            lock (_lock)
            {
                return _logs.TakeLast(count).Reverse().ToList();
            }
        }

        private void Log(string level, string source, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            };

            lock (_lock)
            {
                _logs.Add(entry);
                if (_logs.Count > 1000) _logs.RemoveAt(0);
            }

            OnLogEntry?.Invoke(entry);
        }
    }
}