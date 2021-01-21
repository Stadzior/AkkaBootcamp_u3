using System;
using Octokit;

namespace GithubActors.Models
{
    /// <summary>
    /// used to sort the list of similar repos
    /// </summary>
    public class SimilarRepo : IComparable<SimilarRepo>
    {
        public SimilarRepo(Repository repo)
        {
            Repo = repo;
        }

        public Repository Repo { get; }

        public int SharedStarrers { get; set; }

        public int CompareTo(SimilarRepo other)
            => SharedStarrers.CompareTo(other.SharedStarrers);
    }
}
