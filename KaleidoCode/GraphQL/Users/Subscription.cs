using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace KaleidoCode.GraphQL.Users;

[ExtendObjectType(typeof(Subscription))]
public class UserSubscription
{
    [Subscribe(With = nameof(UserPublished))]
    [Topic("userCreated")]
    public User UserCreated([EventMessage] User user)
    {
        Console.WriteLine("resolve user create message, user id: " + user.Id);
        return user;
    }

    [Subscribe(MessageType = typeof(User))]
    public ValueTask<ISourceStream<User>> UserPublished([Service] ITopicEventReceiver receiver)
    {
        var topic = $"userCreated";
        return receiver.SubscribeAsync<User>(topic);
    }
}
