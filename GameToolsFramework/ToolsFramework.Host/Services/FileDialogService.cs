using Microsoft.Win32;
using ToolsFramework.SDK.Interfaces;

namespace ToolsFramework.Host.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string title, string filter = "All files|*.*")
        {
            var dialog = new OpenFileDialog { Title = title, Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? OpenFolder(string title)
        {
            var dialog = new OpenFolderDialog { Title = title };
            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public string? SaveFile(string title, string filter = "All files|*.*", string defaultName = "")
        {
            var dialog = new SaveFileDialog { Title = title, Filter = filter, FileName = defaultName };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}