using System;
using System.Net;
using System.Windows.Forms;
using Akka.Actor;

namespace GithubActors
{
    static class Program
    {
        /// <summary>
        /// ActorSystem we'llbe using to collect and process data
        /// from Github using their official .NET SDK, Octokit
        /// </summary>
        public static ActorSystem GithubActors;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            GithubActors = ActorSystem.Create("GithubActors");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GithubAuth());
        }
    }
}
