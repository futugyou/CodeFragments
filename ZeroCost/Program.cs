
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// https://mp.weixin.qq.com/s/S8fxe6fNH5GRyf_2rsStQA
// https://github.com/futugyou/doc/blob/master/other/readme.md
using NativeBuffer<int> buff = new(new[] { 1, 2, 3, 4, 5, 6 });
Console.WriteLine(buff[3]);
foreach (var item in buff)
{
    Console.Write(item + " ");
}
foreach (ref var item in buff)
{
    item++;
}
foreach (var item in buff)
{
    Console.Write(item + " ");
}

Console.WriteLine(Color.Parse("#FFEA23")); // Color { R = 255, G = 234, B = 35, A = 0 }

Color color = new(255, 128, 42, 137);
//ColorView view = color.CreateView();

//Console.WriteLine(color); // Color { R = 255, G = 128, B = 42, A = 137 }

//view.R = 7;
//view[3] = 28;
//Console.WriteLine(color); // Color { R = 7, G = 128, B = 42, A = 28 }

//view.Rgba = 3072;
//Console.WriteLine(color); // Color { R = 0, G = 12, B = 0, A = 0 }

//foreach (ref byte i in view) i++;
//Console.WriteLine(color); // Color { R = 1, G = 13, B = 1, A = 1 }


public sealed class NativeBuffer<T> : IDisposable where T : unmanaged
{
    private unsafe T* pointer;
    public nuint Length { get; }

    public NativeBuffer(nuint length)
    {
        Length = length;
        unsafe
        {
            pointer = (T*)NativeMemory.Alloc(length);
        }
    }

    public NativeBuffer(Span<T> span) : this((nuint)span.Length)
    {
        unsafe
        {
            fixed (T* ptr = span)
            {
                Buffer.MemoryCopy(ptr, pointer, sizeof(T) * span.Length, sizeof(T) * span.Length);
            }
        }
    }

    [DoesNotReturn] private ref T ThrowOutOfRange() => throw new IndexOutOfRangeException();

    public ref T this[nuint index]
    {
        get
        {
            unsafe
            {
                return ref (index >= Length ? ref ThrowOutOfRange() : ref (*(pointer + index)));
            }
        }
    }

    public void Dispose()
    {
        unsafe
        {
            // 判断内存是否有效
            if (pointer != (T*)0)
            {
                NativeMemory.Free(pointer);
                pointer = (T*)0;
            }
        }
    }

    // 即使没有调用 Dispose 也可以在 GC 回收时释放资源
    ~NativeBuffer()
    {
        Dispose();
    }

    public NativeBufferEnumerator GetEnumerator()
    {
        unsafe
        {
            return new(ref pointer, Length);
        }
    }

    public ref struct NativeBufferEnumerator
    {
        // this code vs not show error ,but it can not build
        private readonly nuint length;
        private ref T current;
        private nuint index;
        private unsafe readonly ref T* pointer;

        public ref T Current
        {
            get
            {
                unsafe
                {
                    // 确保指向的内存仍然有效
                    if (pointer == (T*)0)
                    {
                        return ref Unsafe.NullRef<T>();
                    }
                    else return ref current;
                }
            }
        }

        public unsafe NativeBufferEnumerator(ref T* pointer, nuint length)
        {
            this.pointer = ref pointer;
            this.length = length;
            this.index = 0;
            this.current = ref Unsafe.NullRef<T>();
        }

        public bool MoveNext()
        {
            unsafe
            {
                // 确保没有越界并且指向的内存仍然有效
                if (index >= length || pointer == (T*)0)
                {
                    return false;
                }

                if (Unsafe.IsNullRef(ref current)) current = ref *pointer;
                else current = ref Unsafe.Add(ref current, 1);
            }
            index++;
            return true;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Color : IParsable<Color>, IEquatable<Color>
{
    public byte R, G, B, A;

    public Color(byte r, byte g, byte b, byte a = 0)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public override string ToString() => $"Color {{ R = {R}, G = {G}, B = {B}, A = {A} }}";
    public override bool Equals(object? other) => other is Color color ? Equals(color) : false;
    public bool Equals(Color other) => (R, G, B, A) == (other.R, other.G, other.B, other.A);
    public static byte[] Serialize(Color color)
    {
        unsafe
        {
            byte[] buffer = new byte[sizeof(Color)];
            MemoryMarshal.Write(buffer, ref color);
            return buffer;
        }
    }

    public static Color Deserialize(ReadOnlySpan<byte> data)
    {
        return MemoryMarshal.Read<Color>(data);
    }

    [DoesNotReturn] private static void ThrowInvalid() => throw new InvalidDataException("Invalid color string.");

    public static Color Parse(string s, IFormatProvider? provider = null)
    {
        if (s.Length is not 7 and not 9 || (s.Length > 0 && s[0] != '#'))
        {
            ThrowInvalid();
        }

        return new()
        {
            R = byte.Parse(s[1..3], NumberStyles.HexNumber, provider),
            G = byte.Parse(s[3..5], NumberStyles.HexNumber, provider),
            B = byte.Parse(s[5..7], NumberStyles.HexNumber, provider),
            A = s.Length is 9 ? byte.Parse(s[7..9], NumberStyles.HexNumber, provider) : default
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Color result)
    {
        result = default;
        if (s?.Length is not 7 and not 9 || (s.Length > 0 && s[0] != '#'))
        {
            return false;
        }

        Color color = new Color();
        return byte.TryParse(s[1..3], NumberStyles.HexNumber, provider, out color.R)
            && byte.TryParse(s[3..5], NumberStyles.HexNumber, provider, out color.G)
            && byte.TryParse(s[5..7], NumberStyles.HexNumber, provider, out color.B)
            && (s.Length is 9 ? byte.TryParse(s[7..9], NumberStyles.HexNumber, provider, out color.A) : true);
    }

    //public ColorView CreateView() => new(ref this);
}

public ref struct ColorView
{
    private readonly ref Color color;

    public ColorView(ref Color color)
    {
        this.color = ref color;
    }

    [DoesNotReturn] private static ref byte ThrowOutOfRange() => throw new IndexOutOfRangeException();

    public ref byte R => ref color.R;
    public ref byte G => ref color.G;
    public ref byte B => ref color.B;
    public ref byte A => ref color.A;
    public ref uint Rgba => ref Unsafe.As<Color, uint>(ref color);
    public ref byte this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return ref color.R;
                case 1:
                    return ref color.G;
                case 2:
                    return ref color.B;
                case 3:
                    return ref color.A;
                default:
                    return ref ThrowOutOfRange();
            }
        }
    }

    public ColorViewEnumerator GetEnumerator()
    {
        return new(this);
    }

    public ref struct ColorViewEnumerator
    {
        private readonly ColorView view;
        private int index;

        public ref byte Current => ref view[index];

        public ColorViewEnumerator(ColorView view)
        {
            this.index = -1;
            this.view = view;
        }

        public bool MoveNext()
        {
            if (index >= 3) return false;
            index++;
            return true;
        }
    }
}