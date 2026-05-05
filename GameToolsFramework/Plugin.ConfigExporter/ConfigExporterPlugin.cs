using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Plugin.ConfigExporter.Views;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.ConfigExporter
{
    public class ConfigExporterPlugin : IPlugin
    {
        public string Id => "config-exporter";
        public string Name => "Config Exporter";
        public string Description => "Converts game configuration files between JSON, CSV and XML formats with live preview";
        public string Version => "1.0.0";
        public string Author => "Tools Team";
        public string Icon => "📦";

        private ILogService? _log;
        private IFileDialogService? _fileDialog;

        public void Initialize(IServiceProvider services)
        {
            _log = services.GetService<ILogService>();
            _fileDialog = services.GetService<IFileDialogService>();
            _log?.Info(Name, "Plugin initialized");
        }

        public UserControl CreateView()
        {
            var view = new ConfigExporterView();
            if (_log != null && _fileDialog != null)
                view.SetServices(_log, _fileDialog);
            return view;
        }

        public void Shutdown() => _log?.Info(Name, "Plugin shutdown");
    }
}