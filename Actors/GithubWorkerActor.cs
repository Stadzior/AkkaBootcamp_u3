using System; 
using System.Linq;
using Akka.Actor;
using GithubActors.Messages;
using Octokit;

namespace GithubActors.Actors
{
    /// <summary>
    /// Individual actor responsible for querying the Github API
    /// </summary>
    public class GithubWorkerActor : ReceiveActor
    {
        private IGitHubClient _gitHubClient;
        private readonly Func<IGitHubClient> _gitHubClientFactory;

        public GithubWorkerActor(Func<IGitHubClient> gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
            InitialReceives();
        }

        protected override void PreStart()
        {
            _gitHubClient = _gitHubClientFactory();
        }

        private void InitialReceives()
        {
            //query an individual starrer
            Receive<RetryableQuery>(query => query.Query is QueryStarrer, query =>
            {
                // ReSharper disable once PossibleNullReferenceException (we know from the previous IS statement that this is not null)
                var starrer = (query.Query as QueryStarrer).Login;
                var sender = Sender;
                try
                {
                    _gitHubClient.Activity.Starring
                        .GetAllForUser(starrer)
                        .ContinueWith<object>(task =>
                        {
                            // query faulted
                            if (task.IsFaulted || task.IsCanceled)
                                return query.NextTry();
                            // query succeeded
                            return new StarredReposForUser(starrer, task.Result);
                        })
                        .PipeTo(sender);
                }
                catch (Exception)
                {
                    //operation failed - let the parent know
                    sender.Tell(query.NextTry());
                }
            });

            //query all starrers for a repository
            Receive<RetryableQuery>(query => query.Query is QueryStarrers, query =>
            {
                // ReSharper disable once PossibleNullReferenceException (we know from the previous IS statement that this is not null)
                var starrers = (query.Query as QueryStarrers).Key;
                var sender = Sender;
                try
                {
                    _gitHubClient.Activity.Starring
                        .GetAllStargazers(starrers.Owner, starrers.Repo)
                        .ContinueWith<object>(task =>
                        {
                            // query faulted
                            if (task.IsFaulted || task.IsCanceled)
                                return query.NextTry();
                            return task.Result.ToArray();
                        })
                        .PipeTo(sender);
                }
                catch (Exception)
                {
                    //operation failed - let the parent know
                    sender.Tell(query.NextTry());
                }
            });
        }
    }
}
