using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Plugin.AssetValidator.Views;
using ToolsFramework.SDK.Interfaces;

namespace Plugin.AssetValidator
{
    public class AssetValidatorPlugin : IPlugin
    {
        public string Id => "asset-validator";
        public string Name => "Asset Validator";
        public string Description => "Scans game asset folders and validates files for common issues — empty files, naming conventions, corrupted images, invalid JSON.";
        public string Version => "1.0.0";
        public string Author => "Tools Team";
        public string Icon => "🔍";

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
            var view = new AssetValidatorView();
            if (_log != null && _fileDialog != null)
                view.SetServices(_log, _fileDialog);
            return view;
        }

        public void Shutdown()
        {
            _log?.Info(Name, "Plugin shutdown");
        }
    }
}