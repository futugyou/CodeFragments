namespace DataflowActor
{
    public interface IActor
    {
        void Send(IMessage message);
        void Handle(IMessage message);
    }
}