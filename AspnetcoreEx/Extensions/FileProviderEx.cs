using Microsoft.Extensions.FileProviders;

namespace AspnetcoreEx.Extensions;

public interface IFileSystem
{
    void ShowStructure(Action<int, string> print);

    void WatchFile(string path);
}
public class FileSystem : IFileSystem
{
    private readonly IFileProvider fileProvider;
    private string filePath;

    public FileSystem(IFileProvider fileProvider)
    {
        this.fileProvider = fileProvider;
    }

    public void ShowStructure(Action<int, string> print)
    {
        int index = -1;
        Print("");
        void Print(string subpath)
        {
            index++;
            foreach (var fileinfo in fileProvider.GetDirectoryContents(subpath))
            {
                print(index, fileinfo.Name);
                if (fileinfo.IsDirectory)
                {
                    Print($@"{subpath}\{fileinfo.Name}".TrimStart('\\'));
                }
            }
            index--;
        }
    }

    public void WatchFile(string path)
    {
        filePath = path;
        ChangeToken.OnChange(()=>fileProvider.Watch(path), Callback);
    }
    async void Callback()
    {
        using var stream = fileProvider.GetFileInfo(filePath).CreateReadStream();
        var buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer);
        var current = Encoding.Default.GetString(buffer);
        Console.WriteLine(current);
    }
}