using System.IO;
using System.Windows;
using System.Windows.Controls;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.AssetValidator.Views
{
    public partial class AssetValidatorView : UserControl
    {
        private ILogService? _log;
        private IFileDialogService? _fileDialog;

        public AssetValidatorView()
        {
            InitializeComponent();
        }

        public void SetServices(ILogService log, IFileDialogService fileDialog)
        {
            _log = log;
            _fileDialog = fileDialog;
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = _fileDialog?.OpenFolder("Select Game Assets Folder");
            if (string.IsNullOrEmpty(folder)) return;

            TxtFolderPath.Text = folder;
            _log?.Info("AssetValidator", $"Scanning folder: {folder}");
            ValidateFolder(folder);
        }

        private void ValidateFolder(string path)
        {
            var results = new List<ValidationResult>();
            int passed = 0, warnings = 0, errors = 0;

            try
            {
                var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var ext = fileInfo.Extension.ToLower();
                    var relativePath = Path.GetRelativePath(path, file);

                    // check empty files
                    if (fileInfo.Length == 0)
                    {
                        results.Add(new ValidationResult("❌", relativePath, "File is empty (0 bytes)", "0 B"));
                        errors++;
                        continue;
                    }

                    // check extremely large files
                    if (fileInfo.Length > 100 * 1024 * 1024)
                    {
                        results.Add(new ValidationResult("⚠️", relativePath,
                            $"File exceeds 100MB — consider optimizing", FormatSize(fileInfo.Length)));
                        warnings++;
                        continue;
                    }

                    // check naming conventions such as spaces or special chars
                    if (fileInfo.Name.Contains(' ') || fileInfo.Name.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.'))
                    {
                        results.Add(new ValidationResult("⚠️", relativePath,
                            "Filename contains spaces or special characters — use underscores", FormatSize(fileInfo.Length)));
                        warnings++;
                        continue;
                    }

                    // check image validation
                    if (ext is ".png")
                    {
                        var bytes = new byte[8];
                        using var fs = File.OpenRead(file);
                        fs.Read(bytes, 0, 8);
                        if (bytes[0] != 0x89 || bytes[1] != 0x50)
                        {
                            results.Add(new ValidationResult("❌", relativePath,
                                "Invalid PNG header — file may be corrupted", FormatSize(fileInfo.Length)));
                            errors++;
                            continue;
                        }
                    }

                    // check JSON validity
                    if (ext is ".json")
                    {
                        try
                        {
                            var content = File.ReadAllText(file);
                            Newtonsoft.Json.Linq.JToken.Parse(content);
                        }
                        catch
                        {
                            results.Add(new ValidationResult("❌", relativePath,
                                "Invalid JSON syntax", FormatSize(fileInfo.Length)));
                            errors++;
                            continue;
                        }
                    }

                    // Passed all checks
                    results.Add(new ValidationResult("✅", relativePath, "Valid", FormatSize(fileInfo.Length)));
                    passed++;
                }
            }
            catch (Exception ex)
            {
                _log?.Error("AssetValidator", $"Scan failed: {ex.Message}");
            }

            ResultsList.ItemsSource = results;
            TxtSummary.Text = $"Scanned {results.Count} files — ✅ {passed} passed, ⚠️ {warnings} warnings, ❌ {errors} errors";
            _log?.Info("AssetValidator", $"Validation complete: {passed} passed, {warnings} warnings, {errors} errors");
        }

        private static string FormatSize(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024):F1} MB"
        };
    }

    public class ValidationResult
    {
        public string StatusIcon { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public string FileSize { get; set; }

        public ValidationResult(string icon, string file, string msg, string size)
        {
            StatusIcon = icon; FileName = file; Message = msg; FileSize = size;
        }
    }
}