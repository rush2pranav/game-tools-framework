using System.Windows.Controls;

namespace ToolsFramework.SDK.Interfaces
{

    // This is the Interface Segregation Principle in action, the plugins only need to implement what's relevant, and the host only depends on this abstraction
    public interface IPlugin
    {
        // unique identifier for the plugin
        string Id { get; }

        // display name shown in the host UI
        string Name { get; }

        // short description of what the plugin does
        string Description { get; }

        // plugin version string
        string Version { get; }

        // author or the team name
        string Author { get; }

        // icon displayed in the plugin list
        string Icon { get; }

        // creates the WPF UserControl that serves as the plugin's ui
        UserControl CreateView();

        void Initialize(IServiceProvider services);
        
        // called when the plugin is being unloaded and used for cleanup
        void Shutdown();
    }
}