namespace ToolsFramework.SDK.Interfaces
{
    // shared dialog file service 
    public interface IFileDialogService
    {
        string? OpenFile(string title, string filter = "All files|*.*");
        string? OpenFolder(string title);
        string? SaveFile(string title, string filter = "All files|*.*", string defaultName = "");
    }
}