using Akka.Actor;

namespace GithubActors.Messages
{
    public class LaunchRepoResultsWindow
    {
        public LaunchRepoResultsWindow(RepoKey repo, IActorRef coordinator)
        {
            Repo = repo;
            Coordinator = coordinator;
        }

        public RepoKey Repo { get; }

        public IActorRef Coordinator { get; }
    }
}
