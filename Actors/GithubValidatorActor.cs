using System;
using System.Linq;
using Akka.Actor;
using GithubActors.Messages;
using Octokit;

namespace GithubActors.Actors
{
    /// <summary>
    /// Actor has one job - ensure that a public repo exists at the specified address
    /// </summary>
    public class GithubValidatorActor : ReceiveActor
    {
        private readonly IGitHubClient _gitHubClient;

        public GithubValidatorActor(IGitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
            ReadyToValidate();
        }

        private void ReadyToValidate()
        {
            //Outright invalid URLs
            Receive<ValidateRepo>(repo => string.IsNullOrEmpty(repo.RepoUri) || !Uri.IsWellFormedUriString(repo.RepoUri, UriKind.Absolute),
                repo => Sender.Tell(new InvalidRepo(repo.RepoUri, "Not a valid absolute URI")));

            //Repos that at least have a valid absolute URL
            Receive<ValidateRepo>(validateRepo =>
            {
                var (owner, repo) = SplitIntoOwnerAndRepo(validateRepo.RepoUri);
                //close over the sender in an instance variable
                var sender = Sender;
                _gitHubClient.Repository
                    .Get(owner, repo)
                    .ContinueWith<object>(t =>
                {
                    //Rule #1 of async in Akka.NET - turn exceptions into messages your actor understands
                    if (t.IsCanceled)
                        return new InvalidRepo(validateRepo.RepoUri, "Repo lookup timed out");
                    if (t.IsFaulted)
                        return new InvalidRepo(validateRepo.RepoUri, t.Exception != null ? t.Exception.GetBaseException().Message : "Unknown Octokit error");

                    return t.Result;
                }).PipeTo(Self, sender);
            });

            // something went wrong while querying github, sent to ourselves via PipeTo
            // however - Sender gets preserved on the call, so it's safe to use Forward here.
            Receive<InvalidRepo>(repo => Sender.Forward(repo));

            // Octokit was able to retrieve this repository
            Receive<Repository>(repository =>
            {
                //ask the GithubCommander if we can accept this job
                Context.ActorSelection(ActorPaths.GithubCommanderActor.Path).Tell(new CanAcceptJob(new RepoKey(repository.Owner.Login, repository.Name)));
            });


            /* REPO is valid, but can we process it at this time? */

            //yes
            Receive<UnableToAcceptJob>(job => Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(job));
            
            //no
            Receive<AbleToAcceptJob>(job => Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(job));
        }

        private (string Owner, string Repo) SplitIntoOwnerAndRepo(string repoUri)
        {
            var split = new Uri(repoUri, UriKind.Absolute)
                .PathAndQuery
                .TrimEnd('/')
                .Split('/')
                .Reverse()
                .ToList(); //uri path without trailing slash
            return (split[1], split[0]); //User, Repo
        }
    }
}
