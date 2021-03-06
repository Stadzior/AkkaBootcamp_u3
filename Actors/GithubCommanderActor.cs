﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using GithubActors.Messages;

namespace GithubActors.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching repo-processing jobs
    /// </summary>
    public class GithubCommanderActor : ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _broadcaster;
        private IActorRef _canAcceptJobSender;

        public IStash Stash { get; set; }
        private int _pendingJobReplies;
        private RepoKey _repoJob;

        private readonly TimeSpan _updatesFrequency = TimeSpan.FromMilliseconds(200);

        public GithubCommanderActor()
            => Ready();

        private void Ready()
        {
            Receive<CanAcceptJob>(job =>
            {
                _broadcaster.Tell(job);
                _repoJob = job.Repo;
                BecomeAsking();
            });
        }
        private void BecomeAsking()
        {
            _canAcceptJobSender = Sender;    
            // block, but ask the router for the number of routees. Avoids magic numbers.
            _pendingJobReplies = _broadcaster
                .Ask<Routees>(new GetRoutees())
                .Result.Members.Count();
            Become(Asking);

            // send ourselves a ReceiveTimeout message if no message within 3 seconds
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(3));
        }

        private void Asking()
        {
            // stash any subsequent requests
            Receive<CanAcceptJob>(job => Stash.Stash());

            Receive<UnableToAcceptJob>(job =>
            {
                _pendingJobReplies--;
                if (_pendingJobReplies > 0)
                    return;

                _canAcceptJobSender.Tell(job);
                BecomeReady();
            });

            Receive<AbleToAcceptJob>(job =>
            {
                _canAcceptJobSender.Tell(job);

                //start processing messages
                Sender.Tell(new BeginJob(job.Repo));

                //launch the new window to view results of the processing
                Context.ActorSelection(ActorPaths.MainFormActor.Path)
                    .Tell(new LaunchRepoResultsWindow(job.Repo, Sender));

                BecomeReady();
            });

            // add this inside the GithubCommanderActor.Asking method
            // means at least one actor failed to respond
            Receive<ReceiveTimeout>(timeout =>
            {
                _canAcceptJobSender.Tell(new UnableToAcceptJob(_repoJob));
                BecomeReady();
            });
        }
        private void BecomeReady()
        {
            Become(Ready);
            Stash.UnstashAll();

            // cancel ReceiveTimeout
            Context.SetReceiveTimeout(null);
        }

        protected override void PreStart()
        {
            _broadcaster = Context.ActorOf(Props
                .Create(() => new GithubCoordinatorActor(_updatesFrequency))
                .WithRouter(FromConfig.Instance), ActorPaths.GithubCoordinatorActor.Name);

            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //kill off the old coordinator so we can recreate it from scratch
            _broadcaster.Tell(PoisonPill.Instance);
            base.PreRestart(reason, message);
        }
    }
}
