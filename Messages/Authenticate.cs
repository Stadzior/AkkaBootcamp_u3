namespace GithubActors.Messages
{
    public class Authenticate
    {
        public Authenticate(string oAuthToken)
        {
            OAuthToken = oAuthToken;
        }

        public string OAuthToken { get; }
    }

}
