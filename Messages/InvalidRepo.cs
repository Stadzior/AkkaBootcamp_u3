namespace GithubActors.Messages
{
    public class InvalidRepo
    {
        public InvalidRepo(string repoUri, string reason)
        {
            Reason = reason;
            RepoUri = repoUri;
        }

        public string RepoUri { get; }

        public string Reason { get; }
    }
}
