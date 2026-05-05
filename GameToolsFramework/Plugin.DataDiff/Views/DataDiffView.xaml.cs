using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.DataDiff.Views
{
    public partial class DataDiffView : UserControl
    {
        private ILogService? _log;
        private IFileDialogService? _fileDialog;
        private string? _file1Path;
        private string? _file2Path;

        public DataDiffView()
        {
            InitializeComponent();
        }

        public void SetServices(ILogService log, IFileDialogService fileDialog)
        {
            _log = log;
            _fileDialog = fileDialog;
        }

        private void BtnFile1_Click(object sender, RoutedEventArgs e)
        {
            _file1Path = _fileDialog?.OpenFile("Select File A (Original)", "Config files|*.json;*.csv|All files|*.*");
            if (_file1Path != null)
            {
                TxtFile1.Text = Path.GetFileName(_file1Path);
                TryCompare();
            }
        }

        private void BtnFile2_Click(object sender, RoutedEventArgs e)
        {
            _file2Path = _fileDialog?.OpenFile("Select File B (Modified)", "Config files|*.json;*.csv|All files|*.*");
            if (_file2Path != null)
            {
                TxtFile2.Text = Path.GetFileName(_file2Path);
                TryCompare();
            }
        }

        private void TryCompare()
        {
            if (_file1Path == null || _file2Path == null) return;

            var ext = Path.GetExtension(_file1Path).ToLower();
            List<DiffResult> diffs;

            if (ext == ".json")
                diffs = CompareJson(_file1Path, _file2Path);
            else if (ext == ".csv")
                diffs = CompareCsv(_file1Path, _file2Path);
            else
            {
                TxtSummary.Text = "Unsupported file format. Use JSON or CSV.";
                return;
            }

            DiffList.ItemsSource = diffs;
            int added = diffs.Count(d => d.ChangeType == "Added");
            int removed = diffs.Count(d => d.ChangeType == "Removed");
            int modified = diffs.Count(d => d.ChangeType == "Modified");
            int unchanged = diffs.Count(d => d.ChangeType == "Unchanged");

            TxtSummary.Text = $"Comparison complete — ➕ {added} added, ➖ {removed} removed, ✏️ {modified} modified, ✓ {unchanged} unchanged";
            _log?.Info("DataDiff", $"Compared {Path.GetFileName(_file1Path)} vs {Path.GetFileName(_file2Path)}: {diffs.Count} fields analyzed");
        }

        private List<DiffResult> CompareJson(string path1, string path2)
        {
            var diffs = new List<DiffResult>();
            try
            {
                var json1 = JObject.Parse(File.ReadAllText(path1));
                var json2 = JObject.Parse(File.ReadAllText(path2));

                var allKeys = json1.Properties().Select(p => p.Name)
                    .Union(json2.Properties().Select(p => p.Name)).Distinct();

                foreach (var key in allKeys)
                {
                    var val1 = json1[key]?.ToString() ?? "";
                    var val2 = json2[key]?.ToString() ?? "";

                    if (!json1.ContainsKey(key))
                        diffs.Add(new DiffResult("Added", key, "", val2));
                    else if (!json2.ContainsKey(key))
                        diffs.Add(new DiffResult("Removed", key, val1, ""));
                    else if (val1 != val2)
                        diffs.Add(new DiffResult("Modified", key, val1, val2));
                    else
                        diffs.Add(new DiffResult("Unchanged", key, val1, val2));
                }
            }
            catch (Exception ex)
            {
                _log?.Error("DataDiff", $"JSON comparison failed: {ex.Message}");
            }
            return diffs;
        }

        private List<DiffResult> CompareCsv(string path1, string path2)
        {
            var diffs = new List<DiffResult>();
            try
            {
                var lines1 = File.ReadAllLines(path1);
                var lines2 = File.ReadAllLines(path2);
                int maxLines = Math.Max(lines1.Length, lines2.Length);

                for (int i = 0; i < maxLines; i++)
                {
                    var line1 = i < lines1.Length ? lines1[i] : "";
                    var line2 = i < lines2.Length ? lines2[i] : "";

                    if (i >= lines1.Length)
                        diffs.Add(new DiffResult("Added", $"Line {i + 1}", "", line2));
                    else if (i >= lines2.Length)
                        diffs.Add(new DiffResult("Removed", $"Line {i + 1}", line1, ""));
                    else if (line1 != line2)
                        diffs.Add(new DiffResult("Modified", $"Line {i + 1}", line1, line2));
                    else
                        diffs.Add(new DiffResult("Unchanged", $"Line {i + 1}", line1, line2));
                }
            }
            catch (Exception ex)
            {
                _log?.Error("DataDiff", $"CSV comparison failed: {ex.Message}");
            }
            return diffs;
        }
    }

    public class DiffResult
    {
        public string ChangeType { get; set; }
        public string Key { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ChangeIcon => ChangeType switch
        {
            "Added" => "➕",
            "Removed" => "➖",
            "Modified" => "✏️",
            _ => "✓"
        };

        public DiffResult(string type, string key, string old, string newVal)
        {
            ChangeType = type; Key = key; OldValue = old; NewValue = newVal;
        }
    }
}