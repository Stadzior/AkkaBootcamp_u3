namespace GithubActors.Messages
{
    public class AbleToAcceptJob
    {
        public AbleToAcceptJob(RepoKey repo)
        {
            Repo = repo;
        }

        public RepoKey Repo { get; }
    }
}
