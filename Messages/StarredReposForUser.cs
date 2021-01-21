using System.Collections.Generic;
using Octokit;

namespace GithubActors.Messages
{
    public class StarredReposForUser
    {
        public StarredReposForUser(string login, IEnumerable<Repository> repos)
        {
            Repos = repos;
            Login = login;
        }

        public string Login { get; }

        public IEnumerable<Repository> Repos { get; }
    }
}
