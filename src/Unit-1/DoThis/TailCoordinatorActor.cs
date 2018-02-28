using System;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types
        /// <summary>
        /// Start tailing the file at user-specified path.
        /// </summary>
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; }

            public IActorRef ReporterActor { get; }
        }

        /// <summary>
        /// Stop tailing the file at user-specified path.
        /// </summary>
        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case StartTail msg:
                    // here we are creating our first parent/child relationship!
                    // the TailActor instance created here is a child
                    // of this instance of TailCoordinatorActor
                    Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
                    break;
            }
        }

        // here we are overriding the default SupervisorStrategy
        // which is a One-For-One strategy w/ a Restart directive
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy (
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30),
                localOnlyDecider: x =>
                {
                    switch (x)
                    {
                        case ArithmeticException _:
                            return Directive.Resume;
                        case NotSupportedException _:
                            return Directive.Stop;
                        default:
                            return Directive.Restart;
                    }
                });
        }
    }
}


