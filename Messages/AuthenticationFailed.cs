namespace GithubActors.Messages
{
    public class AuthenticationFailed
    {
        public AuthenticationFailed(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}
