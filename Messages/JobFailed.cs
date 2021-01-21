namespace GithubActors.Messages
{
    /// <summary>
    /// Let the subscribers know we failed
    /// </summary>
    public class JobFailed
    {
        public JobFailed(RepoKey repo)
        {
            Repo = repo;
        }

        public RepoKey Repo { get; }
    }

}
