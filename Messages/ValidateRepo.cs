namespace GithubActors.Messages
{
    public class ValidateRepo
    {
        public ValidateRepo(string repoUri)
        {
            RepoUri = repoUri;
        }

        public string RepoUri { get; }
    }
}
