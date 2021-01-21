namespace GithubActors.Messages
{
    public class BeginJob
    {
        public BeginJob(RepoKey repo)
        {
            Repo = repo;
        }

        public RepoKey Repo { get; }
    }
}
