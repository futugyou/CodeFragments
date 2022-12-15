namespace AspnetcoreEx.MiniAspnetCore;

public interface IServer
{
    Task StartAsync(RequestDelegate handler);
}