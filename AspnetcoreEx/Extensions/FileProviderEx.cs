﻿using Microsoft.Extensions.Primitives;
using System.Text;

namespace AspnetcoreEx.Extensions;

public interface IFileSystem
{
    void ShowStructure(Action<int, string> print);
    void WatchFile(string path);
    Task<string> ReadAllTextAsync(string path);
}
public class FileSystem : IFileSystem
{
    private readonly IFileProvider fileProvider;
    private string filePath;

    public FileSystem(IFileProvider fileProvider)
    {
        this.fileProvider = fileProvider;
    }

    public async Task<string> ReadAllTextAsync(string path)
    {
        using var stream = fileProvider.GetFileInfo(path).CreateReadStream();
        var buffer = new byte[stream.Length];
        int bytesRead;
        int totalBytesRead = 0;
        while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead))) > 0)
        {
            totalBytesRead += bytesRead;
        }
        return Encoding.Default.GetString(buffer);
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
        ChangeToken.OnChange(() => fileProvider.Watch(path), Callback);
    }
    async void Callback()
    {
        using var stream = fileProvider.GetFileInfo(filePath).CreateReadStream();
        var buffer = new byte[stream.Length];
        int bytesRead;
        int totalBytesRead = 0;
        while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead))) > 0)
        {
            totalBytesRead += bytesRead;
        }
        var current = Encoding.Default.GetString(buffer);
        Console.WriteLine(current);
    }
}