namespace GithubActors.Messages
{
    public class CanAcceptJob
    {
        public CanAcceptJob(RepoKey repo)
        {
            Repo = repo;
        }

        public RepoKey Repo { get; }
    }
}
