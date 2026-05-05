using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ToolsFramework.Host.Commands;
using ToolsFramework.Host.Services;
using ToolsFramework.SDK.Interfaces;
using ToolsFramework.SDK.Services;

namespace ToolsFramework.Host.ViewModels
{
    public class HostViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly ILogService _log;

        public HostViewModel()
        {
            // building the dependency injection container
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILogService, LogService>();
            serviceCollection.AddSingleton<IFileDialogService, FileDialogService>();
            _services = serviceCollection.BuildServiceProvider();

            _log = _services.GetRequiredService<ILogService>();
            _log.OnLogEntry += entry =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    LogEntries.Insert(0, entry);
                    if (LogEntries.Count > 200) LogEntries.RemoveAt(LogEntries.Count - 1);
                });
            };

            Plugins = new ObservableCollection<IPlugin>();
            LogEntries = new ObservableCollection<LogEntry>();

            RefreshPluginsCommand = new RelayCommand(_ => LoadPlugins());

            _log.Info("Host", "Game Tools Framework initialized");
            LoadPlugins();
        }

        // required ccommands
        public ICommand RefreshPluginsCommand { get; }

        // for the collections
        public ObservableCollection<IPlugin> Plugins { get; }
        public ObservableCollection<LogEntry> LogEntries { get; }

        // for the properties
        private IPlugin? _selectedPlugin;
        public IPlugin? SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPluginSelected));
                if (value != null)
                {
                    CurrentPluginView = value.CreateView();
                    _log.Info("Host", $"Activated plugin: {value.Name}");
                }
                else
                {
                    CurrentPluginView = null;
                }
            }
        }

        private UserControl? _currentPluginView;
        public UserControl? CurrentPluginView
        {
            get => _currentPluginView;
            set { _currentPluginView = value; OnPropertyChanged(); }
        }

        public bool HasPluginSelected => SelectedPlugin != null;

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // for the methods
        private void LoadPlugins()
        {
            // Shutdown existing plugins
            foreach (var plugin in Plugins)
            {
                try { plugin.Shutdown(); } catch { }
            }
            Plugins.Clear();
            CurrentPluginView = null;
            _selectedPlugin = null;

            // determine plugin directory
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var pluginDir = Path.Combine(exeDir, "Plugins");

            var loader = new PluginLoader(_log, _services);
            var loaded = loader.LoadPlugins(pluginDir);

            foreach (var plugin in loaded)
                Plugins.Add(plugin);

            StatusText = $"{Plugins.Count} plugins loaded from {pluginDir}";
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}