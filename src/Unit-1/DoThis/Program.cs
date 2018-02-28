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
              event-stream = on
              unhandled = on
        }
    }  
}";
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .MinimumLevel.Verbose()
                .CreateLogger();

            MyActorSystem = ActorSystem.Create("MyActorSystem", AkkaConfig);

            var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
