using System.IO;
using System.Reflection;
using ToolsFramework.SDK.Interfaces;

namespace ToolsFramework.Host.Services
{

    /* discovers and loads plugin DLLs at runtime using reflection.
       This is the core of the plugin architecture:
       1. Scans a "Plugins" folder for DLL files
       2. Loads each assembly
       3. Finds classes that implement IPlugin
       4. Creates instances and initializes them with shared services
     
       The host app has ZERO compile-time knowledge of specific plugins
       New plugins can be added by simply dropping a DLL in the folder
    */

    public class PluginLoader
    {
        private readonly ILogService _log;
        private readonly IServiceProvider _services;

        public PluginLoader(ILogService log, IServiceProvider services)
        {
            _log = log;
            _services = services;
        }

        // discovers and loads the plugins from the given directory
        public List<IPlugin> LoadPlugins(string pluginDirectory)
        {
            var plugins = new List<IPlugin>();

            if (!Directory.Exists(pluginDirectory))
            {
                _log.Warning("PluginLoader", $"Plugin directory not found: {pluginDirectory}");
                Directory.CreateDirectory(pluginDirectory);
                return plugins;
            }

            var dllFiles = Directory.GetFiles(pluginDirectory, "Plugin.*.dll");
            _log.Info("PluginLoader", $"Scanning {pluginDirectory} — found {dllFiles.Length} plugin DLLs");

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(type)!;
                        plugin.Initialize(_services);
                        plugins.Add(plugin);
                        _log.Info("PluginLoader", $"Loaded: {plugin.Name} v{plugin.Version} ({plugin.Id})");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("PluginLoader", $"Failed to load {Path.GetFileName(dllPath)}: {ex.Message}");
                }
            }

            _log.Info("PluginLoader", $"Total plugins loaded: {plugins.Count}");
            return plugins;
        }
    }
}