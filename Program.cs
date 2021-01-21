using System;
using System.Windows.Forms;
using Akka.Actor;

namespace GithubActors
{
    internal static class Program
    {
        /// <summary>
        /// ActorSystem we'll be using to collect and process data
        /// from Github using their official .NET SDK, Octokit
        /// </summary>
        public static ActorSystem GithubActors;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            GithubActors = ActorSystem.Create("GithubActors");
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GithubAuth());
        }
    }
}
