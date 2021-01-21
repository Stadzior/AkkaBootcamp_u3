namespace GithubActors.Messages
{
    public class QueryStarrers
    {
        public QueryStarrers(RepoKey key)
        {
            Key = key;
        }

        public RepoKey Key { get; }
    }
}
