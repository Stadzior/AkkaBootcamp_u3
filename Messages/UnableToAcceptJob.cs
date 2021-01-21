using System;
using System.Collections.Generic;
using System.Text;

namespace GithubActors.Messages
{
    public class UnableToAcceptJob
    {
        public UnableToAcceptJob(RepoKey repo)
        {
            Repo = repo;
        }

        public RepoKey Repo { get; }
    }
}
