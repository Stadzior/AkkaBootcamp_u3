namespace GithubActors.Messages
{
    /// <summary>
    /// Made singleton to prevent garbage collection
    /// </summary>
    public class PublishUpdate
    {
        private PublishUpdate()
        {
        }

        public static PublishUpdate Instance => new PublishUpdate();
    }
}
