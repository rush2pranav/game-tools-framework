using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.ConfigExporter.Views
{
    public partial class ConfigExporterView : UserControl
    {
        private ILogService? _log;
        private IFileDialogService? _fileDialog;
        private string? _sourcePath;
        private JToken? _loadedData;

        public ConfigExporterView()
        {
            InitializeComponent();
        }

        public void SetServices(ILogService log, IFileDialogService fileDialog)
        {
            _log = log;
            _fileDialog = fileDialog;
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            _sourcePath = _fileDialog?.OpenFile("Select Config File", "Config files|*.json;*.csv;*.xml|All files|*.*");
            if (string.IsNullOrEmpty(_sourcePath)) return;

            try
            {
                var content = File.ReadAllText(_sourcePath);
                var ext = Path.GetExtension(_sourcePath).ToLower();

                if (ext == ".json")
                    _loadedData = JToken.Parse(content);
                else if (ext == ".csv")
                    _loadedData = CsvToJson(content);
                else if (ext == ".xml")
                    _loadedData = XmlToJson(content);

                TxtSourceFile.Text = $"{Path.GetFileName(_sourcePath)} ({ext})";
                TxtPreview.Text = _loadedData?.ToString(Newtonsoft.Json.Formatting.Indented) ?? content;
                TxtStatus.Text = $"Loaded: {Path.GetFileName(_sourcePath)}";
                _log?.Info("ConfigExporter", $"Loaded file: {_sourcePath}");
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Error loading file: {ex.Message}";
                _log?.Error("ConfigExporter", $"Load failed: {ex.Message}");
            }
        }

        private void BtnExportJson_Click(object sender, RoutedEventArgs e) => Export("json");
        private void BtnExportCsv_Click(object sender, RoutedEventArgs e) => Export("csv");
        private void BtnExportXml_Click(object sender, RoutedEventArgs e) => Export("xml");

        private void Export(string format)
        {
            if (_loadedData == null)
            {
                TxtStatus.Text = "Load a file first!";
                return;
            }

            var filter = format switch
            {
                "json" => "JSON|*.json",
                "csv" => "CSV|*.csv",
                "xml" => "XML|*.xml",
                _ => "All|*.*"
            };

            var defaultName = Path.GetFileNameWithoutExtension(_sourcePath ?? "export") + $".{format}";
            var savePath = _fileDialog?.SaveFile($"Export as {format.ToUpper()}", filter, defaultName);
            if (string.IsNullOrEmpty(savePath)) return;

            try
            {
                string output = format switch
                {
                    "json" => _loadedData.ToString(Newtonsoft.Json.Formatting.Indented),
                    "csv" => JsonToCsv(_loadedData),
                    "xml" => JsonToXml(_loadedData),
                    _ => _loadedData.ToString()
                };

                File.WriteAllText(savePath, output);
                TxtPreview.Text = output;
                TxtStatus.Text = $"Exported to: {Path.GetFileName(savePath)}";
                _log?.Info("ConfigExporter", $"Exported {format.ToUpper()}: {savePath}");
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Export failed: {ex.Message}";
                _log?.Error("ConfigExporter", $"Export failed: {ex.Message}");
            }
        }

        private static JArray CsvToJson(string csv)
        {
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return new JArray();

            var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            var array = new JArray();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',').Select(v => v.Trim().Trim('"')).ToArray();
                var obj = new JObject();
                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                    obj[headers[j]] = values[j];
                array.Add(obj);
            }
            return array;
        }

        private static JToken XmlToJson(string xml)
        {
            var doc = XDocument.Parse(xml);
            return JToken.Parse(JsonConvert.SerializeXNode(doc, Newtonsoft.Json.Formatting.Indented));
        }

        private static string JsonToCsv(JToken data)
        {
            var array = data is JArray arr ? arr : new JArray { data };
            if (!array.Any()) return "";

            var headers = array.First!.Children<JProperty>().Select(p => p.Name).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers));

            foreach (var item in array)
                sb.AppendLine(string.Join(",", headers.Select(h => $"\"{item[h]?.ToString() ?? ""}\"")));

            return sb.ToString();
        }

        private static string JsonToXml(JToken data)
        {
            var array = data is JArray arr ? arr : new JArray { data };
            var root = new XElement("root");

            foreach (var item in array)
            {
                var element = new XElement("item");
                foreach (var prop in item.Children<JProperty>())
                    element.Add(new XElement(prop.Name.Replace(" ", "_"), prop.Value.ToString()));
                root.Add(element);
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
        }
    }
}