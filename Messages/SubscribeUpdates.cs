using Akka.Actor;

namespace GithubActors.Messages
{
    public class SubscribeUpdates
    {
        public SubscribeUpdates(IActorRef subscriber)
        {
            Subscriber = subscriber;
        }

        public IActorRef Subscriber { get; }
    }
}
