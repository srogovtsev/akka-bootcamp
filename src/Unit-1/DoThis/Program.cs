using System;
﻿using Akka.Actor;
using Serilog;

namespace WinTail
{
    #region Program
    class Program
    {
        private const string AkkaConfig = 
@"akka { 
    loglevel = DEBUG
    loggers = [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
    log-config-on-start = off        
    actor {                
        debug {  
              receive = on 
              autoreceive = on
              lifecycle = on
              event-stream = off
              unhandled = on
        }
    }  
}";
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} [{SourceContext}] {Message:l}{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .CreateLogger();

            MyActorSystem = ActorSystem.Create("MyActorSystem", AkkaConfig);

            var consoleWriterProps = Props.Create<ConsoleWriterActor>();
            var consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            var tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            var tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            var validationActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            var validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");

            var consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            var consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
