using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace AspnetcoreEx.GraphQL;
public class Subscription
{
    [Subscribe]
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

public class SubscriptionType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field("userCreated")
            .Type<UserType>()
            .Resolve(context => context.GetEventMessage<User>())
            .Subscribe(async context =>
            {
                var receiver = context.Service<ITopicEventReceiver>();
                var stream = await receiver.SubscribeAsync<User>("userCreated");
                return stream;
            });
    }
}