﻿using System;
using Akka.Actor;
using GithubActors.Messages;

namespace GithubActors.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching repo-processing jobs
    /// </summary>
    public class GithubCommanderActor : ReceiveActor
    {
        private IActorRef _coordinator;
        private IActorRef _canAcceptJobSender;
        private readonly TimeSpan _updatesFrequency = TimeSpan.FromMilliseconds(500);

        public GithubCommanderActor()
        {
            Receive<CanAcceptJob>(job =>
            {
                _canAcceptJobSender = Sender;
                _coordinator.Tell(job);
            });

            Receive<UnableToAcceptJob>(job => _canAcceptJobSender.Tell(job));

            Receive<AbleToAcceptJob>(job =>
            {
                _canAcceptJobSender.Tell(job);

                //start processing messages
                _coordinator.Tell(new BeginJob(job.Repo));

                //launch the new window to view results of the processing
                Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(new LaunchRepoResultsWindow(job.Repo, Sender));
            });
        }

        protected override void PreStart()
        {
            _coordinator = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor(_updatesFrequency)), ActorPaths.GithubCoordinatorActor.Name);
            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //kill off the old coordinator so we can recreate it from scratch
            _coordinator.Tell(PoisonPill.Instance);
            base.PreRestart(reason, message);
        }
    }
}
