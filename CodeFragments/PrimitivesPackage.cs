
using Microsoft.Extensions.Primitives;

namespace CodeFragments;

public class PrimitivesPackage
{
    public static void Token1()
    {
        CancellationTokenSource cancellationTokenSource = new();
        CancellationChangeToken cancellationChangeToken = new(cancellationTokenSource.Token);

        Console.WriteLine($"HasChanged: {cancellationChangeToken.HasChanged}");

        static void callback(object? _) => Console.WriteLine("The callback was invoked.");

        using IDisposable subscription = cancellationChangeToken.RegisterChangeCallback(callback, null);

        cancellationTokenSource.Cancel();

        Console.WriteLine($"HasChanged: {cancellationChangeToken.HasChanged}\n");

        // Outputs:
        //     HasChanged: False
        //     The callback was invoked.
        //     HasChanged: True
    }

    public static void Token2()
    {
        CancellationTokenSource firstCancellationTokenSource = new();
        CancellationChangeToken firstCancellationChangeToken = new(firstCancellationTokenSource.Token);

        CancellationTokenSource secondCancellationTokenSource = new();
        CancellationChangeToken secondCancellationChangeToken = new(secondCancellationTokenSource.Token);

        CancellationTokenSource thirdCancellationTokenSource = new();
        CancellationChangeToken thirdCancellationChangeToken = new(thirdCancellationTokenSource.Token);

        var compositeChangeToken = new CompositeChangeToken(
        [
            firstCancellationChangeToken,
            secondCancellationChangeToken,
            thirdCancellationChangeToken
        ]);

        static void callback(object? state) => Console.WriteLine($"The {state} callback was invoked.");

        // 1st, 2nd, 3rd, and 4th.
        compositeChangeToken.RegisterChangeCallback(callback, "1st");
        compositeChangeToken.RegisterChangeCallback(callback, "2nd");
        compositeChangeToken.RegisterChangeCallback(callback, "3rd");
        compositeChangeToken.RegisterChangeCallback(callback, "4th");

        // It doesn't matter which cancellation source triggers the change.
        // If more than one trigger the change, each callback is only fired once.
        Random random = new();
        int index = random.Next(3);
        CancellationTokenSource[] sources =
        [
            firstCancellationTokenSource,
            secondCancellationTokenSource,
            thirdCancellationTokenSource
        ];
        sources[index].Cancel();

        Console.WriteLine();
        // Outputs: always
        //     The 4th callback was invoked.
        //     The 3rd callback was invoked.
        //     The 2nd callback was invoked.
        //     The 1st callback was invoked.
    }

    public static void Token3()
    {
        CancellationTokenSource cancellationTokenSource = new();
        CancellationChangeToken cancellationChangeToken = new(cancellationTokenSource.Token);

        IChangeToken producer()
        {
            // The producer factory should always return a new change token.
            // If the token's already fired, get a new token.
            if (cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource = new();
                cancellationChangeToken = new(cancellationTokenSource.Token);
            }

            return cancellationChangeToken;
        }

        void consumer() => Console.WriteLine("The callback was invoked.");

        using (ChangeToken.OnChange(producer, consumer))
        {
            cancellationTokenSource.Cancel();
        }

        // Outputs:
        //     The callback was invoked.
    }


    public static void String1()
    {
        var segment = new StringSegment("This a string, within a single segment representation.", 14, 25);

        Console.WriteLine($"Buffer: \"{segment.Buffer}\"");
        Console.WriteLine($"Offset: {segment.Offset}");
        Console.WriteLine($"Length: {segment.Length}");
        Console.WriteLine($"Value: \"{segment.Value}\"");

        Console.Write("Span: \"");
        foreach (char @char in segment.AsSpan())
        {
            Console.Write(@char);
        }
        Console.Write("\"\n");

        // Outputs:
        //     Buffer: "This a string, within a single segment representation."
        //     Offset: 14
        //     Length: 25
        //     Value: " within a single segment "
        //     " within a single segment "

        // use StringTokenizer instead of string.Split
        var tokenizer = new StringTokenizer(segment, [' ', '.']);
        foreach (StringSegment segm in tokenizer)
        {
            Console.WriteLine(segm.Value);
        }

        StringValues values = new(segment.Buffer!.Split([' ', '.']));

        Console.WriteLine($"Count = {values.Count:#,#}");

        foreach (string? value in values)
        {
            Console.WriteLine(value);
        }
    }
}