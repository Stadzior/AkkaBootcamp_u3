using System;
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
        private const int _coordinatorCount = 3;

        private readonly TimeSpan _updatesFrequency = TimeSpan.FromMilliseconds(200);

        public GithubCommanderActor()
            => Ready();

        private void Ready()
        {
            Receive<CanAcceptJob>(job =>
            {
                _broadcaster.Tell(job);
                BecomeAsking();
            });
        }
        private void BecomeAsking()
        {
            _canAcceptJobSender = Sender;
            _pendingJobReplies = 3; //the number of routees
            Become(Asking);
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
        }
        private void BecomeReady()
        {
            Become(Ready);
            Stash.UnstashAll();
        }

        protected override void PreStart()
        {
            // create GithubCoordinatorActor instances
            var coordinators = new List<IActorRef>();
            for (var i = 0; i < _coordinatorCount; i++)
                coordinators.Add(Context.ActorOf(Props.Create(() => new GithubCoordinatorActor(_updatesFrequency)), ActorPaths.GithubCoordinatorActor.Name + i));

            // create a broadcast router who will ask all of them 
            // if they're available for work
            var broadcastGroup = new BroadcastGroup(coordinators.Select(coordinator => coordinator.Path.ToString()));
            _broadcaster = Context.ActorOf(Props.Empty.WithRouter(broadcastGroup));

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
