using Microsoft.Extensions.FileProviders;

namespace AspnetcoreEx.Extensions;

public interface IFileSystem
{
    void ShowStructure(Action<int, string> print);
}
public class FileSystem : IFileSystem
{
    private readonly IFileProvider fileProvider;

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
}