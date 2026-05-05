using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Plugin.DataDiff.Views;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.DataDiff
{
    public class DataDiffPlugin : IPlugin
    {
        public string Id => "data-diff";
        public string Name => "Data Diff Tool";
        public string Description => "Compares two JSON or CSV config files side by side and highlights added, removed and modified fields";
        public string Version => "1.0.0";
        public string Author => "Tools Team";
        public string Icon => "📊";

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
            var view = new DataDiffView();
            if (_log != null && _fileDialog != null)
                view.SetServices(_log, _fileDialog);
            return view;
        }

        public void Shutdown() => _log?.Info(Name, "Plugin shutdown");
    }
}