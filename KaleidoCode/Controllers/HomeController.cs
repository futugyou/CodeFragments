namespace KaleidoCode.Controllers;

public class HomeController
{
    public Result Foo(string x, int y, double z) => new Result(x, y, z);

    [Microsoft.AspNetCore.Mvc.HttpGet("bar/{x}/{y}/{z}")]
    public ValueTask<Result> Bar(string x, int y, double z) => ValueTask.FromResult(new Result(x, y, z));
}

public record Result(string X, int Y, double Z);