namespace GithubActors.Messages
{
    public class RepoKey
    {
        public RepoKey(string owner, string repo)
        {
            Repo = repo;
            Owner = owner;
        }

        public string Owner { get; }

        public string Repo { get; }
    }
}
