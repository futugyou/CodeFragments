using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeFragments;
public static class Mmf
{
    public static void Write()
    {
        using var mmf = MemoryMappedFile.OpenExisting("ImgA");
        using var accessor = mmf.CreateViewAccessor(4000000, 2000000);
        int colorSize = Marshal.SizeOf(typeof(MyColor));
        MyColor color;

        // Make changes to the view.
        for (long i = 0; i < 1500000; i += colorSize)
        {
            accessor.Read(i, out color);
            color.Brighten(20);
            accessor.Write(i, ref color);
        }

    }
    static long offset = 0x10000000; // 256 megabytes
    static long length = 0x20000000; // 512 megabytes
    public static void Read()
    {
        using var mmf = MemoryMappedFile.CreateFromFile(@"ExtremelyLargeImage.data", FileMode.Open, "ImgA");
        // Create a random access view, from the 256th megabyte (the offset)
        // to the 768th megabyte (the offset plus length).
        using var accessor = mmf.CreateViewAccessor(offset, length);

        int colorSize = Marshal.SizeOf(typeof(MyColor));
        MyColor color;

        // Make changes to the view.
        for (long i = 0; i < length; i += colorSize)
        {
            accessor.Read(i, out color);
            color.Brighten(10);
            accessor.Write(i, ref color);
        }
    }

}

public struct MyColor
{
    public short Red;
    public short Green;
    public short Blue;
    public short Alpha;

    // Make the view brigher.
    public void Brighten(short value)
    {
        Red = (short)Math.Min(short.MaxValue, (int)Red + value);
        Green = (short)Math.Min(short.MaxValue, (int)Green + value);
        Blue = (short)Math.Min(short.MaxValue, (int)Blue + value);
        Alpha = (short)Math.Min(short.MaxValue, (int)Alpha + value);
    }
}