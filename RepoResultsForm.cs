using System.Windows.Forms;
using Akka.Actor;
using GithubActors.Actors;
using GithubActors.Messages;

namespace GithubActors
{
    public partial class RepoResultsForm : Form
    {
        private IActorRef _formActor;
        private readonly IActorRef _githubCoordinator;
        private readonly RepoKey _repo;

        public RepoResultsForm(IActorRef githubCoordinator, RepoKey repo)
        {
            _githubCoordinator = githubCoordinator;
            _repo = repo;
            InitializeComponent();
        }

        private void RepoResultsForm_Load(object sender, System.EventArgs e)
        {
            _formActor =
                Program.GithubActors.ActorOf(
                    Props.Create(() => new RepoResultsActor(dgUsers, tsStatus, tsProgress))
                        .WithDispatcher("akka.actor.synchronized-dispatcher")); //run on the UI thread

            Text = $@"Repos Similar to {_repo.Owner} / {_repo.Repo}";

            //start subscribing to updates
            _githubCoordinator.Tell(new SubscribeUpdates(_formActor));
        }

        private void RepoResultsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //kill the form actor
            _formActor.Tell(PoisonPill.Instance);
        }
    }
}
