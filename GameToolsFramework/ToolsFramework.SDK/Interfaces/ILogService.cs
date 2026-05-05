namespace ToolsFramework.SDK.Interfaces
{
    public interface ILogService
    {
        void Info(string source, string message);
        void Warning(string source, string message);
        void Error(string source, string message);
        event Action<LogEntry>? OnLogEntry;
        List<LogEntry> GetRecentLogs(int count = 100);
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Level { get; set; } = "INFO";
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string Icon => Level switch
        {
            "INFO" => "ℹ️",
            "WARNING" => "⚠️",
            "ERROR" => "❌",
            _ => "📌"
        };
    }
}